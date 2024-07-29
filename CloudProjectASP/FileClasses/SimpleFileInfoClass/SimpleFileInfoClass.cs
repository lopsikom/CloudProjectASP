namespace CloudProjectASP.FileClasses.SimpleFileInfoClass
{
    public class SimpleFileInfo
    {
        public string Name { get; set; }
        public string Extension { get; set; }
        public long Length { get; set; }
        public DateTime LastWriteTime {  get; set; }
        public SimpleFileInfo(string name, string extension, long length, DateTime lastWriteTime)
        {
            Name = name;
            Extension = extension;
            Length = length;
            LastWriteTime = lastWriteTime;
        }
    }
}
