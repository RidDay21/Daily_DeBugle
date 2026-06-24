namespace DailyDeBugle.Models
{
    /// <summary>
    /// Операция редактирования текста для Operational Transformation.
    /// Объявлена как record, чтобы поддерживать немутирующие копии через `with`.
    /// </summary>
    public record TextOperation
    {
        /// <summary>"insert" | "delete" | "noop"</summary>
        public string Type { get; init; } = string.Empty;
 
        /// <summary>Позиция в тексте (0-based).</summary>
        public int Position { get; init; }
 
        /// <summary>Вставляемый текст (только для insert).</summary>
        public string? Text { get; init; }
 
        /// <summary>Количество удаляемых символов (только для delete).</summary>
        public int Length { get; init; }
 
        /// <summary>ID пользователя, создавшего операцию.</summary>
        public int UserId { get; set; }
 
        /// <summary>
        /// Ревизия документа, относительно которой создана операция.
        /// После трансформации сервер выставляет сюда актуальную ревизию.
        /// </summary>
        public int Revision { get; set; }  // set (не init) — сервис выставляет после трансформации
    }
}