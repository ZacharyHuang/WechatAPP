var app = angular.module('gameApp', ['ngRoute']);

app.config(['$routeProvider', function ($routeProvider) {
    $routeProvider
    .when('/', { templateUrl: '/Home/Home' })
    .when('/join', { templateUrl: '/Home/Join' })
    .when('/create', { templateUrl: '/Home/Create' })
    .when('/game', { templateUrl: '/Home/Game' })
    .otherwise({ redirectTo: '/' });
}])

app.controller('homeController', function ($scope, $route) {
    $scope.$route = $route;
    $scope.title = "Home";
})


app.controller('joinController', function ($scope, $route) {
    $scope.$route = $route;
    $scope.title = "Join";
})


app.controller('createController', function ($scope, $route) {
    $scope.$route = $route;
    $scope.title = "Create";
})


app.controller('gameController', function ($scope, $route) {
    $scope.$route = $route;
    $scope.title = "Game";
})