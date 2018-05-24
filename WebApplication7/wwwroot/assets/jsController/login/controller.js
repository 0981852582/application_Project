var app = angular.module('rootApplication', []);
//jsController
app.controller('LoginApplication', function ($scope, $rootScope, $http, $timeout) {
    $scope.model = {};
    $scope.submit = function () {
        $http.post('/Account/Login', $scope.model).success(function (rs) {
            if (rs.error) {
                $scope.message = rs.title;
                $scope.model.password = undefined;
            } else {
                $timeout(() => {
                    window.location.href = '/';
                }, 500);
            }
        });
    }
});