// pages/game/game.js
const app = getApp()

Page({

  /**
   * 页面的初始数据
   */
  data: {
    active: false,
    stage: "Prepare",
    emptySeatAvatar: "/pics/emptySeat.jpg",
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
    if (this.data.active && this.data.stage == "Prepare") {
      this.updatePlayers()
    }
    setTimeout(this.syncPlayer, this.data.stage == "Prepare" ? 250 : 10000)
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
          console.log("server stage: " + stage)
          if (that.data.stage != stage) {
            console.log("stage change: " + that.data.stage + " -> " + stage)
            that.playSound(that.data.stage, "End")
            that.setData({ stage: stage })
            that.playSound(stage, "Start")
          }
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
    }
    else {
      url = app.globalData.backendHost + "/Room/TakeSeat?roomId=" + this.data.roomId + "&seatNumber=" + seatNumber + "&userId=" + this.data.userId + "&userName=" + this.data.userName + "&avatarUrl=" + this.data.userAvatar
      wx.request({
        url: url,
        success: function (res) {
          if (res.statusCode == 200) {
            that.setData({
              isHost: seatNumber == 1,
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
    }
  },

  getCharacter: function () {
    var that = this
    var readyUrl = app.globalData.backendHost + "/Room/Prepare?roomId=" + this.data.roomId + "&userId=" + this.data.userId
    wx.request({
      url: readyUrl
    })
    var characterUrl = app.globalData.backendHost + "/Game/GetCharacter?roomId=" + this.data.roomId + "&seatNumber=" + this.data.seatNumber
    wx.request({
      url: characterUrl,
      success: function (res) {
        if (res.statusCode == 200) {
          that.setData({ character: res.data })
          wx.showModal({
            title: '身份信息',
            content: res.data,
            showCancel: false
          })
        }
        else {
          wx.showModal({
            title: '错误信息',
            content: res.data.Message,
            showCancel: false
          })
        }
      }
    })
  },

  nightFall: function () {
    var url = app.globalData.backendHost + "/Game/NightFall?roomId=" + this.data.roomId
    wx.request({
      url: url,
      success: function (res) {
        if (res.statusCode != 200) {
          wx.showModal({
            title: '错误信息',
            content: res.data.Message,
            showCancel: false
          })
        }
      }
    })
  },

  nightInfo: function () {
    var url = app.globalData.backendHost + "/Game/GetNightInfo?roomId=" + this.data.roomId
    wx.request({
      url: url,
      success: function (res) {
        if (res.statusCode == 200) {
          var dead = JSON.parse(res.data)
          var content = ""
          if (dead.length == 0) {
            content = "无人死亡" 
          }
          else {
            for (var i=0; i<dead.length; ++i) {
              content += (i == 0 ? "" : "，") + dead[i]
            }
            content += "号玩家死亡"
          }
          wx.showModal({
            title: '昨夜信息',
            content: content,
            showCancel: false
          })
        }
        else {
          wx.showModal({
            title: '错误信息',
            content: res.data.Message,
            showCancel: false
          })
        }
      }
    })
  },

  playSound: function (stage, state) {
    if (stage == "Prepare") {
      if (state == "End") console.log("天黑请闭眼")
    }
    else if (stage == "DayTime") {
      if (state == "Start") console.log("天亮了")
      if (state == "End") console.log("天黑请闭眼")
    }
    else if (stage == "ThiefNight") {
      if (state == "Start") console.log("盗贼请睁眼，盗贼请选牌")
      if (state == "End") console.log("盗贼请闭眼")
    }
    else if (stage == "CupidNight") {
      if (state == "Start") console.log("丘比特请睁眼，丘比特请指定情侣")
      if (state == "End") console.log("丘比特请闭眼")
    }
    else if (stage == "LoverDayTime") {
      if (state == "Start") console.log("所有人请睁眼，所有人请点击查看身份，确认是否被丘比特选中")
      if (state == "End") console.log("所有人请闭眼")
    }
    else if (stage == "LoverNight") {
      if (state == "Start") console.log("情侣请睁眼，情侣请互认")
      if (state == "End") console.log("情侣请闭眼")
    }
    else if (stage == "WerewolfNight") {
      if (state == "Start") console.log("狼人请睁眼，狼人请互认同伴，狼人请杀人")
      if (state == "End") console.log("狼人请闭眼")
    }
    else if (stage == "WitchNight") {
      if (state == "Start") console.log("女巫请睁眼，女巫请用药")
      if (state == "End") console.log("女巫请闭眼")
    }
    else if (stage == "ProphetNight") {
      if (state == "Start") console.log("预言家请睁眼，预言家请验人")
      if (state == "End") console.log("预言家请闭眼")
    }
    else if (stage == "GuardNight") {
      if (state == "Start") console.log("守卫请睁眼，守卫请守人")
      if (state == "End") console.log("守卫请闭眼")
    }
    else if (stage == "DemonNight") {
      if (state == "Start") console.log("恶魔请睁眼，恶魔请验人")
      if (state == "End") console.log("恶魔请闭眼")
    }
  }
})