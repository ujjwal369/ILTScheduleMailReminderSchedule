// ======================================
// <copyright file="ConfigurableParameters.cs" company="Enthralltech Pvt. Ltd.">
//     Copyright (C) 2017 Enthralltech Pvt. Ltd. All rights reserved. Unauthorized copying of this file, via any medium is strictly prohibited Proprietary and confidential.
// </copyright>
// ======================================

using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace IltscheduleMailReminderSchedule.Model
{
    [Table("ConfigurableParameter", Schema = "Masters")]
    public class ConfigurableParameter 
    {
        public int Id { get; set; }

        [Required]
        [MaxLength(500)]
        public string Attribute { get; set; }
        [Required]
        [MaxLength(50)]
        public string Code { get; set; }
        [Required]
        [MaxLength(50)]
        public string Value { get; set; }
        [MaxLength(50)]
        public string FieldType { get; set; }
        [MaxLength(100)]
        public string ParameterType { get; set; }
        [MaxLength(100)]
        public string VisibilityType { get; set; }

    }
}
