// Add this to your _Host.cshtml file or to a JS file that's referenced by your app

window.timerInterval = null;

window.startTimerCallback = function (dotnetHelper) {
    // Clear any existing timer to prevent duplicates
    if (window.timerInterval) {
        clearInterval(window.timerInterval);
    }
    
    // Create a new timer that calls back to .NET every second
    window.timerInterval = setInterval(function () {
        dotnetHelper.invokeMethodAsync('UpdateTimerDisplay');
    }, 1000);
};

window.stopTimerCallback = function () {
    if (window.timerInterval) {
        clearInterval(window.timerInterval);
        window.timerInterval = null;
    }
};