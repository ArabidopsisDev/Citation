using Citation.Properties;

namespace Citation.Model.Agriculture
{
    public class Signboard
    {
        public string Id { get; set; } = Guid.NewGuid().ToString();
        public string Content { get; set; } = Resources.SignboardData_DefaultName;
        public double X { get; set; }
        public double Y { get; set; }

        /// <summary>
        /// The ID of the associated field
        /// </summary>
        public string FieldId { get; set; } 
        public DateTime CreateDate { get; set; } = DateTime.Now;
    }
}
