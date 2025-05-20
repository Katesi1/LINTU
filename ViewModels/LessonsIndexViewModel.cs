using LMS.Data.Entities;
using System.Collections.Generic;

namespace LMS.ViewModels
{
    public class LessonsIndexViewModel
    {
        public ClassRoom ClassRoom { get; set; } = null!;
        public List<Lesson> Lessons { get; set; } = new List<Lesson>();
    }
}
