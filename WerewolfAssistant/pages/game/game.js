// pages/game/game.js
const app = getApp()

Page({

  /**
   * 页面的初始数据
   */
  data: {
    syncTimer: null,
    lastSyncPlayer: null,
    lastSyncGame: null,
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
    // this.syncGameStage()
    // this.syncPlayer()
  },

  onShow: function () {
    if (!this.data.syncTimer) {
      this.sync()
    }
  },

  onHide: function () {
    if (this.data.syncTimer) {
      clearTimeout(this.data.syncTimer)
      this.setData({ syncTimer: null })
    }
  },

  onUnload: function () {
    clearTimeout(this.data.syncTimer)
    this.setData({ syncTimer: null })
  },

  sync: function () {
    var that = this
    var d = new Date()
    var now = d.getTime()

    if (!this.data.lastSyncPlayer || now - this.data.lastSyncPlayer >= (this.data.stage == "Prepare" ? 250 : 10000)) {
      this.updatePlayers()
      this.setData({ lastSyncPlayer: now })
    }

    if (!this.data.lastSyncGame || now - this.data.lastSyncGame >= 1000) {
      this.updateGameStage()
      this.setData({ lastSyncGame: now })
    }

    this.setData({ syncTimer: setTimeout(this.sync, 250) })
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
          for (var i=1; i<=players.length; ++i) {
            if (players[i] && players[i].UserId == that.data.userId) {
              that.setData({
                isHost: i == 1,
                seatNumber: i
              })
              break;
            }
          }
        }
      },
    })
    if (this.data.stage != "Prepare" && !this.data.character) {
      var characterUrl = app.globalData.backendHost + "/Game/GetCharacter?roomId=" + this.data.roomId + "&seatNumber=" + this.data.seatNumber
      wx.request({
        url: characterUrl,
        success: function (res) {
          if (res.statusCode == 200) {
            that.setData({ character: res.data })
          }
        }
      })
    }
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
    var seatNumber = Number(e.currentTarget.dataset.seatnumber)

    if (this.data.stage == "Prepare") {
      if (this.data.players[seatNumber] && this.data.players[seatNumber].UserId == this.data.userId) {
        this.leaveSeat(seatNumber)
      }
      else {
        this.takeSeat(seatNumber)
      }
    }
    else if (this.data.stage == "CupidNight") {

    }
    else if (this.data.stage == "WerewolfNight") {
      this.werewolfSkill(seatNumber)
    }
  },

  takeSeat(seatNumber) {
    var url = app.globalData.backendHost + "/Room/TakeSeat?roomId=" + this.data.roomId + "&seatNumber=" + seatNumber + "&userId=" + this.data.userId + "&userName=" + this.data.userName + "&avatarUrl=" + this.data.userAvatar
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
  },

  leaveSeat: function (seatNumber) {
    var url = app.globalData.backendHost + "/Room/LeaveSeat?roomId=" + this.data.roomId + "&userId=" + this.data.userId
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
  },

  updateCharacer: function () {
    var url = app.globalData.backendHost + "/Game/GetCharacter?roomId=" + this.data.roomId + "&seatNumber=" + this.data.seatNumber
    wx.request({
      url: url,
      success: function (res) {
        if (res.statusCode == 200) {
          that.setData({ character: res.data })
        }
        else {
          wx.showModal({
            title: '无法获得身份信息',
            content: res.data.Message,
            showCancel: false
          })
        }
      }
    })
  },

  getCharacter: function () {
    var that = this
    var readyUrl = app.globalData.backendHost + "/Room/Prepare?roomId=" + this.data.roomId + "&userId=" + this.data.userId
    wx.request({
      url: readyUrl
    })
    this.updateCharacter()
    if (this.data.character && this.data.character != "") {
      wx.showModal({
        title: '身份信息',
        content: this.data.character,
        showCancel: false
      })
    }
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
        console.log(res)
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

  werewolfSkill: function (target) {
    var that = this
    wx.showModal({
      title: '目标确认',
      content: "确认杀死" + target + "号玩家？",
      success: function () {
        var url = app.globalData.backendHost + "/Game/WerewolfSkill?roomId=" + that.data.roomId + "&seatNumber=" + that.data.seatNumber + "&useSkill=true&target=" + target
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
      }
    })
  },

  useSkill: function () {
    var that = this
    if (!this.data.character) {
      this.updateCharacter()
    }

    if (this.data.character == "Thief") {
      this.thiefSkill()
    }

    if (this.data.character == "Cupid") {

    }

    if (this.data.character == "Werewolf" || this.data.character == "Demon" || this.data.character == "WhiteWerewolf") {
      wx.showModal({
        title: '狼人技能',
        content: '使用技能：确认后请点击目标座位\r\n不使用技能：请点击取消',
        fail: function() {
          var url = app.globalData.backendHost + "/Game/WerewolfSkill?roomId=" + that.data.roomId + "&seatNumber=" + that.data.seatNumber + "&useSkill=false&target=0"
        }
      })
    }

    if (this.data.character == "Prophet") {
      
    }
  },

  thiefSkill: function() {
    var that = this
    var url = app.globalData.backendHost + "/Game/GetThiefCandidates?roomId=" + this.data.roomId + "&seatNumber=" + this.data.seatNumber
    wx.request({
      url: url,
      success: function (res) {
        console.log(res)
        if (res.statusCode == 200) {
          var cand = JSON.parse(res.data)
          wx.showModal({
            title: '请选择身份',
            content: "1:" + cand[1] + " or 2:" + cand[0],
            confirmText: "2",
            cancelText: "1",
            success: function () {
              url = app.globalData.backendHost + "/Game/ThiefSkill?roomId=" + that.data.roomId + "&seatNumber=" + that.data.seatNumber + "&useSkill=true&choice=0"
              wx.request({
                url: url,
                success: function (res) {
                  console.log(res)
                }
              })
            },
            fail: function () {
              url = app.globalData.backendHost + "/Game/ThiefSkill?roomId=" + that.data.roomId + "&seatNumber=" + that.data.seatNumber + "&useSkill=true&choice=1"
              wx.request({
                url: url,
                success: function (res) {
                  console.log(res)
                }
              })
            }
          })
        }
      }
    })
  },

  prophetSkill: function (target) {
    var that = this
    wx.showModal({
      title: '目标确认',
      content: "确认检验" + target + "号玩家的身份？",
      success: function () {
        var url = app.globalData.backendHost + "/Game/ProphetSkill?roomId=" + that.data.roomId + "&seatNumber=" + that.data.seatNumber + "&useSkill=true&target=" + target
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
      }
    })
  },

  witchPoison: function (heal, poison, target) {
    wx.showModal({
      title: '目标确认',
      content: "确认毒死" + target + "号玩家？",
      success: function () {
        var url = app.globalData.backendHost + "/Game/WitchSkill?roomId=" + that.data.roomId + "&seatNumber=" + that.data.seatNumber + "&heal=" + heal + "&poison=" + poison + "&target=" + target
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
    else if (stage == "LoversDayTime") {
      if (state == "Start") console.log("所有人请睁眼，请点击查看身份，确认是否被丘比特选中")
      if (state == "End") console.log("所有人请闭眼")
    }
    else if (stage == "LoversNight") {
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