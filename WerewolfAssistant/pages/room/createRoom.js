// pages/room/createRoom.js
const app = getApp()

Page({

  /**
   * 页面的初始数据
   */
  data: {
    playerNumber: 0,
    godNumber: 0,
    villageNumber: 0,
    werewolfNumber: 0,
    allWerewolfNumber: 0,
    prophet: 0,
    witch: 0,
    hunter: 0,
    guard: 0,
    idiot: 0,
    cupid: 0,
    demon: 0,
    whiteWerewolf: 0,
    thief: 0,
    witchHealSelf: 1,
    witchTwoSkillInOneNight: false,
    healAndGuardIsDead: true,
    rule1: "仅首夜",
    rule2: "不能同时用药",
    rule3: "会死"
  },
  updatePlayerNumber: function () {
    var playerNumber = this.data.godNumber + this.data.villageNumber + this.data.allWerewolfNumber - this.data.thief
    this.setData({ playerNumber: playerNumber })
  },

  updateGodNumber: function () {
    var godNumber = this.data.prophet + this.data.witch + this.data.hunter + this.data.guard+ this.data.idiot + this.data.cupid
    this.setData({ godNumber: godNumber })
  },

  updateAllWerewolfNumber: function () {
    var allWerewolfNumber = this.data.demon + this.data.whiteWerewolf + this.data.werewolfNumber
    this.setData({ allWerewolfNumber: allWerewolfNumber })
  },

  tapProphet: function() {
    this.setData({ prophet: this.data.prophet > 0 ? 0 : 1 })
    this.updateGodNumber()
    this.updatePlayerNumber()
  },

  tapWitch: function () {
    this.setData({ witch: this.data.witch > 0 ? 0 : 1 })
    this.updateGodNumber()
    this.updatePlayerNumber()
  },

  tapHunter: function () {
    this.setData({ hunter: this.data.hunter > 0 ? 0 : 1 })
    this.updateGodNumber()
    this.updatePlayerNumber()
  },

  tapGuard: function () {
    this.setData({ guard: this.data.guard > 0 ? 0 : 1 })
    this.updateGodNumber()
    this.updatePlayerNumber()
  },

  tapIdiot: function () {
    this.setData({ idiot: this.data.idiot > 0 ? 0 : 1 })
    this.updateGodNumber()
    this.updatePlayerNumber()
  },

  tapCupid: function () {
    this.setData({ cupid: this.data.cupid > 0 ? 0 : 1 })
    this.updateGodNumber()
    this.updatePlayerNumber()
  },

  tapDemon: function () {
    this.setData({ demon: this.data.demon > 0 ? 0 : 1 })
    this.updateAllWerewolfNumber()
    this.updatePlayerNumber()
  },

  tapWhiteWerewolf: function () {
    this.setData({ whiteWerewolf: this.data.whiteWerewolf > 0 ? 0 : 1 })
    this.updateAllWerewolfNumber()
    this.updatePlayerNumber()
  },

  tapThief: function () {
    this.setData({ thief: this.data.thief > 0 ? 0 : 1 })
    this.updatePlayerNumber()
  },

  increaseWerewolf: function () {
    if (this.data.werewolfNumber < 8) {
      this.setData({ werewolfNumber: this.data.werewolfNumber + 1 })
      this.updateAllWerewolfNumber()
      this.updatePlayerNumber()
    }
  },

  decreaseWerewolf: function () {
    if (this.data.werewolfNumber > 0) {
      this.setData({ werewolfNumber: this.data.werewolfNumber - 1 })
      this.updateAllWerewolfNumber()
      this.updatePlayerNumber()
    }
  },

  increaseVillage: function () {
    if (this.data.werewolfNumber < 10) {
      this.setData({ villageNumber: this.data.villageNumber + 1 })
      this.updatePlayerNumber()
    }
  },

  decreaseVillage: function () {
    if (this.data.villageNumber > 0) {
      this.setData({ villageNumber: this.data.villageNumber - 1 })
      this.updatePlayerNumber()
    }
  },

  tapRule1: function () {
    var witchHealSelf = (this.data.witchHealSelf + 1) % 3
    this.setData({
      rule1: witchHealSelf == 0 ? "不能自救" : witchHealSelf == 1 ? "仅首夜" : "可以自救",
      witchHealSelf: witchHealSelf
    })
  },

  tapRule2: function () {
    var witchTwoSkillInOneNight = !this.data.witchTwoSkillInOneNight
    this.setData({
      rule2: witchTwoSkillInOneNight ? "可以同时用药" : "不能同时用药",
      witchTwoSkillInOneNight: witchTwoSkillInOneNight
    })
  },

  tapRule3: function () {
    var healAndGuardIsDead = !this.data.healAndGuardIsDead
    this.setData({
      rule3: healAndGuardIsDead ? "会死" : "不会死",
      healAndGuardIsDead: healAndGuardIsDead
    })
  },

  submit: function() {
    var gods = (this.data.prophet > 0 ? "预言家，" : "") + (this.data.witch > 0 ? "女巫，" : "") + (this.data.hunter > 0 ? "猎人，" : "") + (this.data.guard > 0 ? "守卫，" : "") + (this.data.idiot > 0 ? "白痴，" : "") + (this.data.cupid > 0 ? "丘比特，" : "")
    var werewolves = (this.data.demon > 0 ? "恶魔，" : "") + (this.data.whiteWerewolf > 0 ? "白狼王，" : "") + this.data.werewolfNumber + "只狼，"
    var villages = "" + this.data.villageNumber + "个村民"
    var rules = "女巫" + this.data.rule1 + (this.data.rule1.endsWith("自救") ? "" : "可以自救") + "，女巫" + this.data.rule2 + "，同守同救" + this.data.rule3
    var content = "配置：" + gods + werewolves + villages + "\r\n规则：" + rules
    var createGameUrl = app.globalData.backendHost + "/Game/CreateGame?villageNumber=" + this.data.villageNumber + "&werewolfNumber=" + this.data.werewolfNumber + "&prophet=" + this.data.prophet + "&witch=" + this.data.witch + "&hunter=" + this.data.hunter + "&guard=" + this.data.guard + "&idiot=" + this.data.idiot + "&cupid=" + this.data.cupid + "&demon=" + this.data.demon
      + "&whiteWerewolf=" + this.data.whiteWerewolf + "&thief=" + this.data.thief + "&witchHealSelf=" + this.data.witchHealSelf + "&witchTwoSkillInOneNight=" + this.data.witchTwoSkillInOneNight + "&healAndGuardIsDead=" + this.data.healAndGuardIsDead
    wx.showModal({
      title: "确认提交？",
      content: content,
      success: function (res) {
        if (res.confirm) {
          wx.request({
            url: createGameUrl,
            success: function (res) {
              if (res.statusCode == 200) {
                wx.redirectTo({
                  url: '../game/game?roomId=' + res.data
                })
              }
              else{
                console.log(res)
                wx.showModal({
                  title: '创建失败',
                  content: res.data.Message,
                  showCancel: false
                })
              }
            }
          })
        }
      }
    })
  }
})