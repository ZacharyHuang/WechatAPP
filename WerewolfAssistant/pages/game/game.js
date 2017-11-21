// pages/game/game.js
const app = getApp()
const innerAudioContext = wx.createInnerAudioContext()
const stageEndSound = {
  "Prepare": "/voices/nightFall.mp3",
  "DayTime": "/voices/nightFall.mp3",
  "ThiefNight": "/voices/thiefEnd.mp3",
  "CupidNight": "/voices/cupidEnd.mp3",
  "LoversDayTime": "/voices/loversDayEnd.mp3",
  "LoversNight": "/voices/loversNightEnd.mp3",
  "WerewolfNight": "/voices/werewolfEnd.mp3",
  "WitchNight": "/voices/witchEnd.mp3",
  "ProphetNight": "/voices/prophetEnd.mp3",
  "GuardNight": "/voices/guardEnd.mp3",
  "DemonNight": "/voices/demonEnd.mp3"
}

const stageBeginSound = {
  "DayTime": "/voices/dayTime.mp3",
  "ThiefNight": "/voices/thiefBegin.mp3",
  "CupidNight": "/voices/cupidBegin.mp3",
  "LoversDayTime": "/voices/loversDayBegin.mp3",
  "LoversNight": "/voices/loversNightBegin.mp3",
  "WerewolfNight": "/voices/werewolfBegin.mp3",
  "WitchNight": "/voices/witchBegin.mp3",
  "ProphetNight": "/voices/prophetBegin.mp3",
  "GuardNight": "/voices/guardBegin.mp3",
  "DemonNight": "/voices/demonBegin.mp3"
}

var isAudioPlaying = false

Page({

  /**
   * 页面的初始数据
   */
  data: {
    syncTimer: null,
    soundTimer: null,
    lastSyncPlayer: null,
    lastSyncGame: null,
    stage: null,
    emptySeatAvatar: "/pics/emptySeat.jpg",
    roomId: "",
    userId: "",
    userName: "",
    userAvatar: "",
    isHost: false,
    seatNumber: -1,
    players: [],
    character: "",
    tapActive: false,
    lover1: null,
    witchHeal: false,
    iter: [0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17, 18, 19, 20],
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
          for (var i = 1; i <= players.length; ++i) {
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
          if (that.data.stage != stage) {
            if (that.data.isHost) {
              that.playSound(stageEndSound[that.data.stage])
            }
            that.setData({ stage: stage })
            if (that.data.isHost) {
              that.playSound(stageBeginSound[stage])
            }
          }
        }
      },
    })
  },

  updateCharacter: function () {
    var that = this
    var url = app.globalData.backendHost + "/Game/GetCharacter?roomId=" + this.data.roomId + "&seatNumber=" + this.data.seatNumber
    wx.request({
      url: url,
      success: function (res) {
        if (res.statusCode == 200) {
          var characterInfo = JSON.parse(res.data)
          that.setData({ character: characterInfo.Character })
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
    if (this.data.stage == "Prepare" && this.data.seatNumber && this.data.players[this.data.seatNumber] != "Ready") {
      var readyUrl = app.globalData.backendHost + "/Room/Prepare?roomId=" + this.data.roomId + "&userId=" + this.data.userId
      wx.request({
        url: readyUrl
      })
    }
    var characterUrl = app.globalData.backendHost + "/Game/GetCharacter?roomId=" + this.data.roomId + "&seatNumber=" + this.data.seatNumber
    wx.request({
      url: characterUrl,
      success: function (res) {
        if (res.statusCode == 200) {
          var characterInfo = JSON.parse(res.data)
          that.setData({ character: characterInfo.Character })
          var content = characterInfo.Character + (characterInfo.IsLover == true ? "\r\nYou are one of the couple" : characterInfo.IsLover == false ? "\r\nYou are not one of the couple" : "")
          wx.showModal({
            title: '身份信息',
            content: content,
            showCancel: false
          })
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
            for (var i = 0; i < dead.length; ++i) {
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

  skillRequest: function (action, useSkill, target, opt) {
    var url = ""
    var that = this
    if (action == "ThiefSkill") {
      url = app.globalData.backendHost + "/Game/" + action + "?roomId=" + this.data.roomId + "&seatNumber=" + this.data.seatNumber + "&useSkill=" + useSkill + "&choice=" + target
    }
    else if (action == "WitchSkill") {
      url = app.globalData.backendHost + "/Game/" + action + "?roomId=" + this.data.roomId + "&seatNumber=" + this.data.seatNumber + "&heal=" + opt + "&poison=" + useSkill + "&target=" + target
    }
    else if (action == "CupidSkill") {
      url = app.globalData.backendHost + "/Game/" + action + "?roomId=" + this.data.roomId + "&seatNumber=" + this.data.seatNumber + "&useSkill=" + useSkill + "&target1=" + target + "&target2=" + opt
    }
    else {
      url = app.globalData.backendHost + "/Game/" + action + "?roomId=" + this.data.roomId + "&seatNumber=" + this.data.seatNumber + "&useSkill=" + useSkill + "&target=" + target
    }
    wx.request({
      url: url,
      success: function (res) {
        if (res.statusCode == 200) {
          if (action == "ProphetSkill" || action == "DemonSkill") {
            wx.showModal({
              title: '身份信息',
              content: res.data,
              showCancel: false
            })
          }
          else if (action == "ThiefSkill") {
            that.updateCharacter()
          }
          that.setData({ tapActive: false })
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

  tapSeat: function (e) {
    var seatNumber = Number(e.currentTarget.dataset.seatnumber)

    if (this.data.stage == "Prepare") {
      if (this.data.players[seatNumber] && this.data.players[seatNumber].UserId == this.data.userId) {
        this.leaveSeat(seatNumber)
      }
      else {
        this.takeSeat(seatNumber)
      }
    }
    if (this.data.tapActive) {
      if (this.data.stage == "CupidNight") {
        this.cupidTap(seatNumber)
      }
      else if (this.data.stage == "WerewolfNight") {
        this.werewolfTap(seatNumber)
      }
      else if (this.data.stage == "WitchNight") {
        this.witchTap(seatNumber)
      }
      else if (this.data.stage == "ProphetNight") {
        this.prophetTap(seatNumber)
      }
      else if (this.data.stage == "GuardNight") {
        this.guardTap(seatNumber)
      }
      else if (this.data.stage == "DemonNight") {
        this.demonTap(seatNumber)
      }
    }
  },

  takeSeat(seatNumber) {
    var that = this
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
    var that = this
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

  cupidTap: function (target) {
    var that = this
    var lover1 = this.data.lover1
    if (lover1) {
      if (target != lover1) {
        wx.showModal({
          title: '目标确认',
          content: "确认连接" + lover1 + "号玩家与" + target + "号玩家成为情侣？",
          success: function (res) {
            if (res.confirm) {
              that.skillRequest("CupidSkill", true, lover1, target)
            }
          }
        })
      }
      this.setData({ lover1: null })
    }
    else {
      this.setData({ lover1: target })
    }
  },

  werewolfTap: function (target) {
    var that = this
    wx.showModal({
      title: '目标确认',
      content: "确认杀死" + target + "号玩家？",
      success: function () {
        that.skillRequest("WerewolfSkill", true, target)
      }
    })
  },

  witchTap: function (target) {
    var that = this
    var witchHeal = this.data.witchHeal
    var content = "确认" + (witchHeal ? "" : "不") + "使用解药并毒死" + target + "号玩家？"
    wx.showModal({
      title: '目标确认',
      content: content,
      success: function () {
        that.skillRequest("WitchSkill", true, target, witchHeal)
      }
    })
  },

  prophetTap: function (target) {
    var that = this
    wx.showModal({
      title: '目标确认',
      content: "确认检验" + target + "号玩家的身份？",
      success: function () {
        that.skillRequest("ProphetSkill", true, target)
      }
    })
  },

  guardTap: function (target) {
    var that = this
    wx.showModal({
      title: '目标确认',
      content: "确认守卫" + target + "号玩家？",
      success: function () {
        that.skillRequest("GuardSkill", true, target)
      }
    })
  },

  demonTap: function (target) {
    var that = this
    wx.showModal({
      title: '目标确认',
      content: "确认检验" + target + "号玩家的身份？",
      success: function () {
        that.skillRequest("DemonSkill", true, target)
      }
    })
  },

  useSkill: function () {
    var that = this
    if (!this.data.character || this.data.character == "") {
      this.updateCharacter()
    }

    if (this.data.character == "Thief" && this.data.stage == "ThiefNight") {
      this.thiefSkill()
    }

    if (this.data.character == "Cupid" && this.data.stage == "CupidNight") {
      this.setData({ tapActive: true })
      wx.showModal({
        title: '丘比特技能',
        content: '确认后请点击目标座位',
        showCancel: false
      })
    }

    if ((this.data.character == "Werewolf" || this.data.character == "Demon" || this.data.character == "WhiteWerewolf") && this.data.stage == "WerewolfNight") {
      this.setData({ tapActive: true })
      wx.showModal({
        title: '狼人技能',
        content: '使用技能：确认后请点击目标座位\r\n不使用技能：请点击取消',
        success: function (res) {
          if (res.cancel) {
            that.skillRequest("WerewolfSkill", false, 0)
          }
        }
      })
    }

    if (this.data.character == "Witch") {
      this.witchSkill()
    }

    if (this.data.character == "Prophet" && this.data.stage == "ProphetNight") {
      this.setData({ tapActive: true })
      wx.showModal({
        title: '预言家技能',
        content: '使用技能：确认后请点击目标座位\r\n不使用技能：请点击取消',
        success: function (res) {
          if (res.cancel) {
            that.skillRequest("ProphetSkill", false, 0)
          }
        }
      })
    }

    if (this.data.character == "Guard" && this.data.stage == "GuardNight") {
      this.setData({ tapActive: true })
      wx.showModal({
        title: '守卫技能',
        content: '使用技能：确认后请点击目标座位\r\n不使用技能：请点击取消',
        success: function (res) {
          if (res.cancel) {
            that.skillRequest("GuardSkill", false, 0)
          }
        }
      })
    }
  },

  thiefSkill: function () {
    var that = this
    var url = app.globalData.backendHost + "/Game/GetThiefCandidates?roomId=" + this.data.roomId + "&seatNumber=" + this.data.seatNumber
    wx.request({
      url: url,
      success: function (res) {
        if (res.statusCode == 200) {
          var cand = JSON.parse(res.data)
          wx.showModal({
            title: '请选择身份',
            content: "1:" + cand[1] + " or 2:" + cand[0],
            confirmText: "2",
            cancelText: "1",
            success: function (res) {
              if (res.confirm) {
                that.skillRequest("ThiefSkill", true, 0)
              }
              else if (res.cancel) {
                that.skillRequest("ThiefSkill", true, 1)
              }
            }
          })
        }
      }
    })
  },

  witchSkill: function () {
    var that = this
    this.setData({ witchHeal: false, tapActive: true })
    var url = app.globalData.backendHost + "/Game/GetWitchInfo?roomId=" + this.data.roomId + "&seatNumber=" + this.data.seatNumber
    wx.request({
      url: url,
      success: function (res) {
        if (res.statusCode == 200) {
          var dead = "", canHeal = false
          if (res.data == "Unknown") {
            dead = "未知"
          }
          else if (res.data == "None") {
            dead = "无人死亡"
          }
          else {
            dead = res.data + "号玩家"
            canHeal = true
          }
          wx.showModal({
            title: '女巫解药',
            content: '昨晚死亡的是：' + dead,
            confirmText: "救",
            cancelText: "不救",
            success: function () {
              if (canHeal) {
                that.setData({ witchHeal: true })
              }
              else {
                wx.showToast({
                  title: '无法使用解药',
                })
              }
            },
            complete: function () {
              wx.showModal({
                title: '女巫毒药',
                content: '使用毒药：确认后请点击目标座位\r\n不使用毒药：请点击取消',
                success: function (res) {
                  if (res.cancel) {
                    that.skillRequest("WitchSkill", false, 0, that.data.witchHeal)
                  }
                },
                complete: function () {
                  that.setData({ witchHeal: null })
                }
              })
            }
          })
        }
      }
    })
  },

  gameOver: function () {
    clearTimeout(this.data.syncTimer)
    this.setData({ syncTimer: null })
    var url = app.globalData.backendHost + "/Game/GameOver?roomId=" + this.data.roomId
    wx.request({
      url: url,
      success: function (res) {
        if (res.statusCode == 200) {
          wx.redirectTo({
            url: '../index/index',
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

  playSound: function (sound) {
    var that = this
    if (sound) {
      if (isAudioPlaying) {
        this.setData({ soundTimer: setTimeout(function () { that.playSound(sound) }, 1000) }) 
      }
      else {
        isAudioPlaying = true
        innerAudioContext.src = sound
        innerAudioContext.onEnded(function () { isAudioPlaying = false })
        innerAudioContext.play()
      }

    }
  }
})