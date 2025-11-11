using DailyDeBugle.Models;

namespace DailyDebugle.Services
{
    public class StyleService
    {
        public TextStyle CurrentStyle { get; private set; } = new TextStyle();
        
        public event Action? OnStyleChanged;
        
        public void UpdateStyle(TextStyle newStyle)
        {
            CurrentStyle = newStyle;
            OnStyleChanged?.Invoke();
        }
        
        public void ResetToDefault()
        {
            CurrentStyle = new TextStyle();
            OnStyleChanged?.Invoke();
        }
    }
}