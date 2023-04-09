namespace CourseLibrary.API.ResourceParameters
{
    public class AuthorsResourceParameters
    {
        //Filtering
        public string? MainCategory { get; set; }
        //Searching
        public string? SearchQuery { get; set; }

        //Pagination
        const int maxPageSize = 20;
        private int _pageSize = 10;
        public int PageNumber { get; set; } = 1;
        public int PageSize
        { 
            get => _pageSize; 
            set => _pageSize = (value > maxPageSize) ? maxPageSize : value; 
        }

        public string OrderBy { get; set; } = "Name";

        public string? Fields { get; set; }
    }
}
