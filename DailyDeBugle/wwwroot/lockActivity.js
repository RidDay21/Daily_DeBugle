window.lockActivity = {
    lastActivityTime: Date.now(),
    refreshInterval: null,
    dotNetHelper: null,

    init: function (dotNetHelper) {
        this.dotNetHelper = dotNetHelper;
        this.trackActivity();
    },

    trackActivity: function () {
        const self = this;
        // Слушаем любую активность пользователя
        document.addEventListener('mousemove', () => self.lastActivityTime = Date.now());
        document.addEventListener('keydown', () => self.lastActivityTime = Date.now());
        document.addEventListener('click', () => self.lastActivityTime = Date.now());
        document.addEventListener('scroll', () => self.lastActivityTime = Date.now());
    },

    hasRecentActivity: function (seconds) {
        return (Date.now() - this.lastActivityTime) < (seconds * 1000);
    }
};