// pages/game/game.js
const app = getApp()

Page({

  /**
   * 页面的初始数据
   */
  data: {
    needSync: false,
    emptySeatAvatar: "../pics/emptySeat.png",
    roomId: "",
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
      userName: userName,
      userAvatar: userAvatar
    })
    
    this.updateGame()
    this.updatePlayers()


  },

  /**
   * 生命周期函数--监听页面初次渲染完成
   */
  onReady: function () {

  },

  onShow: function () {
    this.setData({ needSync: true })
    this.sync()
  },

  onHide: function () {
    this.setData({ needSync: false })
  },

  onUnload: function () {
    this.setData({ needSync: false })
  },

  syncPlayer: function () {
    // if (this.data.needSync) {
    //   this.updatePlayers()
    //   setTimeout(this.sync, 250)
    // }
  },

  syncGame: function () {
    
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

  updateGame: function () {
    var that = this
    var url = app.globalData.backendHost + "/Game/GetGame?roomId=" + this.data.roomId
    wx.request({
      url: url,
      success: function (res) {
        console.log(res)
        if (res.statusCode == 200) {
          var game = JSON.parse(res.data)
          that.setData({ config: game.Config })
        }
      },
    })
  },

  tapSeat: function (e) {
    var that = this
    var url = ""
    var seatNumber = Number(e.currentTarget.dataset.seatnumber)

    if (this.data.players[seatNumber] && this.data.players[seatNumber].UserName == this.data.userName) {
      url = app.globalData.backendHost + "/Room/LeaveSeat?roomId=" + this.data.roomId + "&userId=" + this.data.userName
      wx.request({
        url: url,
        success: function (res) {
          if (res.statusCode == 200) {
            that.updatePlayers()
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
        }
      })
    }
    else {
      url = app.globalData.backendHost + "/Room/TakeSeat?roomId=" + this.data.roomId + "&seatNumber=" + seatNumber + "&userId=" + this.data.userName + "&userName=" + this.data.userName + "&avatarUrl=" + this.data.userAvatar
      wx.request({
        url: url,
        success: function (res) {
          if (res.statusCode == 200) {
            that.updatePlayers()
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
        }
      })

    }
    // if (this.data.players[seatNumber] && this.data.players[seatNumber].name == this.data.userName) {
    //   var players = this.data.Players
    //   players[seatNumber] = null
    //   this.setData({
    //     players: players,
    //     isHost: false
    //   })
    // }
    // else {
    //   var players = this.data.players
    //   for (var i=0; i<players.length; ++i) {
    //     if (players[i] && players[i].name == this.data.userName) {
    //       players[i] = null
    //     }
    //   }
    //   players[seatNumber] = {
    //     name: this.data.userName,
    //     avatar: this.data.userAvatar
    //   }
    //   this.setData({
    //     players: players,
    //     isHost: seatNumber == 0
    //    })
    // }
  },

  start: function () {
    this.setData({ gameStatus: "dayTime" })
  }
})