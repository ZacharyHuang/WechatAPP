// pages/room/joinRoom.js
Page({
  inputRoom: function(e) {
    var roomId = e.detail.value
    if (roomId.length == 6) {
      wx.redirectTo({
        url: '../game/game?roomId=' + roomId,
      })
    }
  }
})