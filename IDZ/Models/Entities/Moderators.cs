//------------------------------------------------------------------------------
// <auto-generated>
//     Этот код создан по шаблону.
//
//     Изменения, вносимые в этот файл вручную, могут привести к непредвиденной работе приложения.
//     Изменения, вносимые в этот файл вручную, будут перезаписаны при повторном создании кода.
// </auto-generated>
//------------------------------------------------------------------------------

namespace IDZ.Models.Entities
{
    using System;
    using System.Collections.Generic;
    
    public partial class Moderators
    {
        public System.Guid moderator_id { get; set; }
        public Nullable<System.Guid> user_id { get; set; }
        public Nullable<System.Guid> article_id { get; set; }
    
        public virtual Articles Articles { get; set; }
        public virtual Users Users { get; set; }
    }
}
