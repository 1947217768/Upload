namespace UploadFile.Models
{
    public class SliceFileInfo
    {
        /// <summary>
        /// 文件名称
        /// </summary>
        public string Name { get; set; }
        /// <summary>
        /// 编号
        /// </summary>
        public int Number { get; set; }
        /// <summary>
        /// 分片大小
        /// </summary>
        public int BufferSize { get; set; }
        /// <summary>
        /// 分片总数
        /// </summary>
        public int Count { get; set; }
        /// <summary>
        /// 文件开始位置
        /// </summary>
        public int Start { get; set; }
        /// <summary>
        /// 文件结束位置
        /// </summary>
        public int End { get; set; }
        /// <summary>
        /// 文件总大小
        /// </summary>
        public int Size { get; set; }
    }
}
