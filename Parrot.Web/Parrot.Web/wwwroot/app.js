window.modalBodyLock = {
    lock: function () {
        document.documentElement.style.overflow = 'hidden';
    },
    unlock: function () {
        document.documentElement.style.overflow = '';
    }
};
