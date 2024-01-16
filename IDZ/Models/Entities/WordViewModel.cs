﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;

namespace IDZ.Models.Entities
{
    public class WordViewModel
    {
        [Required(ErrorMessage = "The Word field is required.")]
        public string Word { get; set; }

        [Required(ErrorMessage = "The Article Content field is required.")]
        public string ArticleContent { get; set; }
    }



}