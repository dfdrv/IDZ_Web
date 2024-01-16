using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using IDZ.Models.Entities;
using System.Net;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using IDZ.Models.ViewModels;

namespace IDZ.Controllers
{
    public class LabController : Controller
    {
        // GET: Lab
        [AllowAnonymous]
        public ActionResult WordsList()
        {
            List<Words> terms = new List<Words>();
            using (var db = new Entities())
            {
                terms = db.Words.OrderBy(x => x.word).ToList();
            }
            return View(terms);
        }
        [Authorize]
        public ActionResult WordDetails(Guid? id)
        {
            using (var db = new Entities())
            {
                if (id != null)
                {
                    var articles = db.Articles.Where(a => a.word_id == id).ToList();
                    return View(articles);
                }
            }

            // Вернуть что-то в случае, если id равен null или статьи не найдены
            return HttpNotFound();
        }

        [Authorize]
        [HttpGet]
        public ActionResult CreateWord()
        {
            return View();
        }
        
        [Authorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult CreateWord(WordViewModel newWord)
        {
            if (ModelState.IsValid)
            {
                using (var context = new Entities())
                {
                    Words word = new Words
                    {
                        word_id = Guid.NewGuid(),
                        word = newWord.Word
                    };
                    context.Words.Add(word);

                    // Создание статьи и привязка к слову
                    Articles article = new Articles
                    {
                        article_id = Guid.NewGuid(),
                        content = newWord.ArticleContent,
                        word_id = word.word_id, // Привязка к слову
                        last_modified_time = DateTime.Now 
                    };
                    context.Articles.Add(article);

                    context.SaveChanges();
                }
                return RedirectToAction("WordsList");
            }

            return View(newWord);
        }
        [Authorize(Roles = "moderator")]
        [HttpGet]
        public ActionResult DeleteWord(Guid? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            using (var context = new Entities())
            {
                Words word = context.Words.Find(id);
                if (word == null)
                {
                    return HttpNotFound();
                }

                // Получите связанные статьи
                List<Articles> articles = context.Articles.Where(a => a.word_id == id).ToList();

                // Создайте модель представления, которая содержит информацию о слове и связанных статьях
                WordDeleteViewModel viewModel = new WordDeleteViewModel
                {
                    Word = word,
                    Articles = articles
                };

                return View(viewModel);
            }
        }
        [Authorize(Roles = "moderator")]
        [HttpPost, ActionName("DeleteWord")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteWordConfirmed(Guid id)
        {
            using (var context = new Entities())
            {
                // Находим слово
                Words word = context.Words.Find(id);
                if (word == null)
                {
                    return HttpNotFound();
                }

                // Находим и удаляем связанные статьи
                List<Articles> articles = context.Articles.Where(a => a.word_id == id).ToList();

                // Удаляем связанные записи в таблице Moderators
                foreach (var article in articles)
                {
                    context.Moderators.RemoveRange(context.Moderators.Where(m => m.article_id == article.article_id));
                }

                // Удаляем связанные статьи
                context.Articles.RemoveRange(articles);

                // Удаляем само слово
                context.Words.Remove(word);
                context.SaveChanges();

                return RedirectToAction("WordsList");
            }
        }
        [Authorize]
        [HttpGet]
        public ActionResult EditWord(Guid? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            using (var context = new Entities())
            {
                // Получение слова и связанной статьи
                Words word = context.Words.Find(id);
                Articles article = context.Articles.FirstOrDefault(a => a.word_id == id);

                if (word == null || article == null)
                {
                    return HttpNotFound();
                }

                // Создание модели представления
                EditWordViewModel viewModel = new EditWordViewModel
                {
                    WordId = word.word_id,
                    Word = word.word,
                    ArticleContent = article.content
                    // Заполните другие свойства статьи, если они есть
                };

                return View(viewModel);
            }
        }
        [Authorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult EditWord(EditWordViewModel model)
        {
            if (ModelState.IsValid)
            {
                using (var context = new Entities())
                {
                    // Получение существующего слова и связанной статьи из базы данных
                    Words existingWord = context.Words.Find(model.WordId);
                    Articles existingArticle = context.Articles.FirstOrDefault(a => a.word_id == model.WordId);

                    if (existingWord != null && existingArticle != null)
                    {
                        // Обновление существующего слова значениями из модели
                        existingWord.word = model.Word;

                        // Создание записи в таблице Edits
                        Edits edit = new Edits
                        {
                            edit_id = Guid.NewGuid(),
                            article_id = existingArticle.article_id,
                            user_id = null, // Укажите идентификатор пользователя, если доступен
                            edit_content = model.ArticleContent,
                            edit_time = DateTime.Now,
                            is_applied = false // Исходно метка "не применено"
                        };

                        // Сохранение записи в таблице Edits
                        context.Edits.Add(edit);

                        // Обновление связанной статьи
                        existingArticle.content = model.ArticleContent;
                        // Обновите другие свойства статьи, если они есть

                        // Сохранение изменений в базе данных
                        context.SaveChanges();
                    }
                    else
                    {
                        // Обработка случая, если слово или связанная статья с указанным ID не найдены
                        // Возможно, стоит перенаправить на страницу ошибки или вернуть соответствующий ответ
                        return HttpNotFound();
                    }
                }

                return RedirectToAction("WordsList");
            }

            return View(model);
        }
        [AllowAnonymous]
        [HttpGet]
        public ActionResult SearchArticleByWord(string searchTerm)
        {
            using (var context = new Entities())
            {
                ViewBag.SearchTerm = searchTerm; // Передаем в представление
                                                 // Ищем статьи, содержащие заданное слово в названии
                var articles = context.Articles
                    .Where(a => a.Words.word.Contains(searchTerm))
                    .ToList();

                return View("SearchResults", articles);
            }
        }


        [Authorize(Roles = "moderator")]
        public ActionResult ArticleHistory(Guid wordId)
        {
            using (var context = new Entities())
            {
                var word = context.Words.Find(wordId);
                if (word == null)
                {
                    return HttpNotFound();
                }

                // Получаем историю изменений для статьи этого слова
                var editsHistory = context.Edits
                    .Where(e => e.Articles.word_id == wordId)
                    .OrderByDescending(e => e.edit_time)
                    .Select(e => new ArticleHistoryViewModel
                    {
                        EditContent = e.edit_content,
                        EditTime = e.edit_time,
                        IsApplied = e.is_applied ?? false
                // Заполните другие атрибуты, если необходимо
            })
                    .ToList();

                ViewBag.Word = word.word; // Передаем слово в представление

                return View(editsHistory);
            }
        }



    }
}