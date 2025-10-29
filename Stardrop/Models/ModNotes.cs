using System.Collections.Generic;
using System.Text.Json;

namespace Stardrop.Models
{
    /// <summary>
    /// 用于存储MOD备注信息的类
    /// </summary>
    public class ModNotes
    {
        /// <summary>
        /// MOD的唯一标识符与备注信息的映射
        /// </summary>
        public Dictionary<string, string> Notes { get; set; }

        public ModNotes()
        {
            Notes = new Dictionary<string, string>();
        }

        /// <summary>
        /// 获取指定MOD的备注信息
        /// </summary>
        /// <param name="uniqueId">MOD的唯一标识符</param>
        /// <returns>备注信息，如果不存在则返回空字符串</returns>
        public string GetNotes(string uniqueId)
        {
            if (Notes.ContainsKey(uniqueId))
            {
                return Notes[uniqueId];
            }
            return string.Empty;
        }

        /// <summary>
        /// 设置指定MOD的备注信息
        /// </summary>
        /// <param name="uniqueId">MOD的唯一标识符</param>
        /// <param name="notes">备注信息</param>
        public void SetNotes(string uniqueId, string notes)
        {
            if (string.IsNullOrEmpty(notes))
            {
                // 如果备注为空，则从字典中移除
                if (Notes.ContainsKey(uniqueId))
                {
                    Notes.Remove(uniqueId);
                }
            }
            else
            {
                // 否则更新或添加备注
                Notes[uniqueId] = notes;
            }
        }
    }
}