using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace IDZ.Models.Entities
{
    public class WordDeleteViewModel
    {
        public Words Word { get; set; }
        public List<Articles> Articles { get; set; }
    }
}