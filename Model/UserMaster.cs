﻿using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace IltscheduleMailReminderSchedule.Model
{
    [Table("UserMaster", Schema = "User")]
    public class UserMaster
    {
        public int Id { get; set; }      
        public string UserName { get; set; }
        public string EmailId { get; set; }
        public string UserRole { get; set; }
        public bool IsActive { get; set; }
        public bool IsDeleted { get; set; }
       
    }
}
