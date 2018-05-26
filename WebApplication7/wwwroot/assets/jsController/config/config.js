var app = angular.module('rootApplication', ['ui.bootstrap', 'ui.router', 'oc.lazyLoad', 'toaster']);
//config angularjs
app.config(['$compileProvider', function ($compileProvider) {
    $compileProvider.debugInfoEnabled(false);
}]);
app.config(function ($httpProvider) {
    $httpProvider.useApplyAsync(1000); //true
});
//directive
// BlockUI
var BlockUI = function (objects) {
    App.blockUI({ target: objects.target, boxed: !0, message: objects.message });
}
var UnBlockUI = function (objects) {
    App.unblockUI(objects);
}
var Comfirm = function (messages, callback) {
    bootbox.dialog({ message: messages, title: "", buttons: { success: { label: "Đồng ý", className: "btn-primary", callback: function () { return callback(true) } }, danger: { label: "Hủy", className: "red", callback: function () { return callback(false) } } } })
}
app.config(['$stateProvider', '$urlRouterProvider', function ($stateProvider, $urlRouterProvider) {
    // Redirect any unmatched url
    $urlRouterProvider.otherwise("");

    $stateProvider.state('permission', {
        url: "/permission",
        templateUrl: "../../assets/jsController/permission/index.html",
        controller: "mainPermisstion",
        resolve: {
            deps: ['$ocLazyLoad', function ($ocLazyLoad) {
                return $ocLazyLoad.load({
                    name: 'lazyLoadApp',
                    files: [
                        '../../assets/jsController/permission/controller.js'
                    ]
                });
            }]
        }
    }).state('menuBar', {
        url: "/menuBar",
        templateUrl: "../../assets/jsController/menuBar/index.html",
        controller: "mainMenubar",
        resolve: {
            deps: ['$ocLazyLoad', function ($ocLazyLoad) {
                return $ocLazyLoad.load({
                    name: 'lazyLoadApp',
                    files: [
                        '../../assets/jsController/menuBar/controller.js'
                    ]
                });
            }]
        }
        }).state('account', {
            url: "/account",
            templateUrl: "../../assets/jsController/account/index.html",
            controller: "mainAccount",
            resolve: {
                deps: ['$ocLazyLoad', function ($ocLazyLoad) {
                    return $ocLazyLoad.load({
                        name: 'lazyLoadApp',
                        files: [
                            '../../assets/jsController/account/controller.js'
                        ]
                    });
                }]
            }
        })
}]);
//controller
app.controller('rootController', function ($scope, $state, $rootScope, $http, $uibModal, $timeout, toaster) {
    $http.post('/MenuBar/getViewMenuBar').success(function (rs) {
        if (rs.error) {
            toaster.pop("error", "", rs.title, 1000, "");
        } else {
            $rootScope.listMenuBar = rs.result;
        }
    });
});
