(function () {
  const eventSoundDict = {
    WrongGuess: new Audio("sounds/Errors and Cancel/Cancel 1.m4a"),
    CorrectGuess: new Audio("sounds/Complete and Success/Success 2.m4a"),
    MyTurn: new Audio("sounds/Notifications and Alerts/Notification 3.m4a"),
    TimeUp: new Audio("sounds/Errors and Cancel/Error 5.m4a"),
    IWin: new Audio("sounds/Notifications and Alerts/Notification 9.m4a"),
    ILose: new Audio("sounds/Errors and Cancel/Error 4.m4a"),
  };
  for (const event in eventSoundDict) {
    window.addEventListener(event, () => {
      eventSoundDict[event].play();
    });
  }
})();
