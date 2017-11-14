// pages/game/game.js
const app = getApp()

Page({

  /**
   * 页面的初始数据
   */
  data: {
    emptySeatAvatar: "../pics/emptySeat.png",
    roomId: "",
    userName: "",
    userAvatar: "",
    isHost: false,
    seatNumber: -1,
    isReady: false,
    players: [],
    character: "",
    iter: [],
    config: null,
    gameStatus: null
  },

  /**
   * 生命周期函数--监听页面加载
   */
  onLoad: function (options) {
    this.setData({ roomId: options.roomId })
    var userName = app.globalData.userInfo.nickName
    var userAvatar = app.globalData.userInfo.avatarUrl
    this.setData({
      userName: userName,
      userAvatar: userAvatar
    })
    this.setData({ characters: ["Village", "Prophet", "Village", "Werewolf", "Werewolf", "Witch", "Village", "Hunter", "Werewolf"]})
    
    var config = {
      playerNumber: 16,
      characterNumber: 16,
      godNumber: 4,
      villageNumber: 4,
      werewolfNumber: 4,
      hasProphet: true,
      hasWitch: true,
      hasHunter: true,
      hasGuard: true,
      hasIdiot: false,
      hasCupid: false,
      hasElder: false,
      hasDemon: false,
      hasWhiteWerewolf: false,
      hasThief: false
    }
    this.setData({ config: config })

    var iter = []
    for (var i=0;i<this.data.config.playerNumber;++i) {
      iter.push(i)
    }
    this.setData({ iter:iter })
  },

  /**
   * 生命周期函数--监听页面初次渲染完成
   */
  onReady: function () {
  
  },

  tapSeat: function (e) {
    var seatNumber = Number(e.currentTarget.dataset.seatnumber)
    if (this.data.players[seatNumber] && this.data.players[seatNumber].name == this.data.userName) {
      var players = this.data.Players
      players[seatNumber] = null
      this.setData({
        players: players,
        isHost: false
      })
    }
    else {
      var players = this.data.players
      for (var i=0; i<players.length; ++i) {
        if (players[i] && players[i].name == this.data.userName) {
          players[i] = null
        }
      }
      players[seatNumber] = {
        name: this.data.userName,
        avatar: this.data.userAvatar
      }
      this.setData({
        players: players,
        isHost: seatNumber == 0
       })
    }
  },

  start: function () {
    this.setData({gameStatus: "dayTime"})
  }
})