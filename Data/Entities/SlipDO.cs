﻿using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Data.Entities
{
    public class SlipDO : BaseDO
    {
        [Key]
        public int Id { get; set; }
        
        public string Name { get; set; }


    }
}
