// pages/game/game.js
const app = getApp()

Page({

  /**
   * 页面的初始数据
   */
  data: {
    active: false,
    stage: "Prepare",
    emptySeatAvatar: "../pics/emptySeat.png",
    roomId: "",
    userId: "",
    userName: "",
    userAvatar: "",
    isHost: false,
    seatNumber: -1,
    isReady: false,
    players: [],
    character: "",
    iter: [0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17, 18, 19, 20],
    config: null,
    gameStatus: null
  },

  /**
   * 生命周期函数--监听页面加载
   */
  onLoad: function (options) {
    var roomId = options.roomId
    var userName = app.globalData.userInfo.nickName
    var userAvatar = app.globalData.userInfo.avatarUrl
    app.globalData.debug = this
    this.setData({
      roomId: roomId,
      userId: userName,
      userName: userName,
      userAvatar: userAvatar
    })
    
    this.updateGameConfig()
    this.updatePlayers()
    this.updateGameStage()
    this.syncGameStage()
    this.syncPlayer()
  },

  onShow: function () {
    this.setData({ active: true })
  },

  onHide: function () {
    this.setData({ active: false })
  },

  onUnload: function () {
    this.setData({ active: false })
  },

  syncPlayer: function () {
    if (this.data.active) {
      this.updatePlayers()
    }
    setTimeout(this.syncPlayer, this.data.stage == "Prepare" ? 250 : 5000)
  },

  syncGameStage: function () {
    if (this.data.active) {
      this.updateGameStage()
    }
    setTimeout(this.syncGameStage, 1000)
  },

  updatePlayers: function () {
    var that = this
    var url = app.globalData.backendHost + "/Room/GetPlayers?roomId=" + this.data.roomId
    wx.request({
      url: url,
      success: function (res) {
        console.log(res)
        if (res.statusCode == 200) {
          var players = JSON.parse(res.data)
          that.setData({ players: players })
        }
      },
    })
  },

  updateGameConfig: function () {
    var that = this
    var url = app.globalData.backendHost + "/Game/GetGameConfig?roomId=" + this.data.roomId
    wx.request({
      url: url,
      success: function (res) {
        console.log(res)
        if (res.statusCode == 200) {
          var config = JSON.parse(res.data)
          that.setData({ config: config })
        }
      },
    })
  },

  updateGameStage: function () {
    var that = this
    var url = app.globalData.backendHost + "/Game/GetGameStage?roomId=" + this.data.roomId
    wx.request({
      url: url,
      success: function (res) {
        console.log(res)
        if (res.statusCode == 200) {
          var stage = res.data
          that.setData({ stage: stage })
        }
      },
    })
  },

  tapSeat: function (e) {
    var that = this
    var url = ""
    var seatNumber = Number(e.currentTarget.dataset.seatnumber)

    if (this.data.players[seatNumber] && this.data.players[seatNumber].UserId == this.data.userId) {
      url = app.globalData.backendHost + "/Room/LeaveSeat?roomId=" + this.data.roomId + "&userId=" + this.data.userId
      wx.request({
        url: url,
        success: function (res) {
          if (res.statusCode == 200) {
            that.setData({
              isHost: false,
              seatNumber: -1
            })
          }
          else {
            wx.showModal({
              title: '离座失败',
              content: res.data.Message,
              showCancel: false
            })
          }
        },
        complete: function () {
          that.updatePlayers()
        }
      })
      // var players = this.data.players
      // players[seatNumber] = null
      // that.setData({ players: players })

    }
    else {
      url = app.globalData.backendHost + "/Room/TakeSeat?roomId=" + this.data.roomId + "&seatNumber=" + seatNumber + "&userId=" + this.data.userId + "&userName=" + this.data.userName + "&avatarUrl=" + this.data.userAvatar
      wx.request({
        url: url,
        success: function (res) {
          if (res.statusCode == 200) {
            that.setData({
              isHost: seatNumber == 0,
              seatNumber: seatNumber
            })
          }
          else {
            wx.showModal({
              title: '占座失败',
              content: res.data.Message,
              showCancel: false
            })
          }
        },
        complete: function () {
          that.updatePlayers()
        }
      })
      // var players = this.data.players 
      // for (var i=0; i<players.length; ++i) {
      //   if (players[i] && players[i].UserId == this.data.userId) {
      //     players[i] = null
      //   }
      // }
      // players[seatNumber] = {
      //   UserId: this.data.userId,
      //   UserName: this.data.userName,
      //   AvatarUrl: this.data.userAvatar
      // }
      // that.setData({ players: players })
    }
  },

  start: function () {
    this.setData({ gameStatus: "dayTime" })
  }
})