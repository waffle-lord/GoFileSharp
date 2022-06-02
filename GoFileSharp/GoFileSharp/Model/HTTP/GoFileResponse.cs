namespace GoFileSharp.Model.HTTP
{
    public class GoFileResponse<T> where T : class
    {
        public string Status = "unknown";
        public T? Data;

        /// <summary>
        /// Just a convenient way to check the status is ok
        /// </summary>
        public bool IsOK => Status == "ok";
    }
}
