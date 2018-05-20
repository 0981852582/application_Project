var app = angular.module('APP', ['ui.bootstrap', 'ui.router', 'oc.lazyLoad', 'toaster']);
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
//controller
app.controller('CONTROLLER', function ($scope, $rootScope, $http, $uibModal, $timeout, toaster) {
    $scope.init = function () {
        var buildDatatable = function () {
            $scope.table = {
                search: '',
                columnOrder: undefined,
                orderType: 'ASC',
                orderClass: 'sorting_asc',
                order: undefined,
                orderFunction: function (string) {
                    if (this.order === string) {
                        if (this.orderType === 'ASC') {
                            this.orderType = 'DESC';
                            this.orderClass = 'sorting_desc';
                        }
                        else {
                            this.orderType = 'ASC';
                            this.orderClass = 'sorting_asc';
                        }
                    } else {
                        this.orderType = 'ASC';
                        this.orderClass = 'sorting_asc';
                    }
                    this.order = string;
                    this.columnOrder = string + ' ' + this.orderType;
                },
                currentPage: 1,
                totalItem: 0,
                numberPage: 5,
                maxSize: 5,
                fromRow: 0,
                endRow: 0

            };
            $scope.table.changeSearch = function () {
                // event change search
                this.DataTable();
            }
            $scope.table.changeOrder = function (column, id) {
                // event change order
                this.orderFunction(column);
                this.DataTable();
                // order by id view
                order(id, this.orderClass);
            }
            $scope.table.changePagging = function () {
                this.DataTable();
            }
            $scope.table.DataTable = function () {
                var cursor = this;
                BlockUI({
                    target: 'body',
                    message: 'Đang tải...'
                });
                $http.post("/Home/getListPermission/", this).success(function (rs) {
                    cursor.totalItem = rs.totalItem;
                    cursor.results = rs.results;
                    cursor.fromRow = rs.fromRow;
                    cursor.endRow = rs.endRow;
                    $timeout(() => {
                        UnBlockUI("body");
                    }, 1);
                });
            }
            $rootScope.reload = function () {
                $scope.table.DataTable();
            }
            $rootScope.reload();
        }
        //inital create table
        buildDatatable();
    }
    // inital init 
    $scope.init();
    // call dialog view
    $scope.dialogView = function (data) {
        /*begin modal*/
        var modalInstance = $uibModal.open({
            templateUrl: '../assets/controller/permission/dialogView.html',
            controller: 'ViewPermission',
            backdrop: 'static',
            size: 'lg',
            resolve: {
                parameter: function () {
                    return data;
                }
            }
        });
    };
    // call dialog update
    $scope.dialogUpdate = function (data) {
        /*begin modal*/
        var modalInstance = $uibModal.open({
            templateUrl: '../assets/controller/permission/dialogUpdate.html',
            controller: 'UpdatePermission',
            backdrop: 'static',
            size: 'lg',
            resolve: {
                parameter: function () {
                    return data;
                }
            }
        });
    };
    // call dialog insert
    $scope.dialogInsert = function () {
        /*begin modal*/
        var modalInstance = $uibModal.open({
            templateUrl: '../assets/controller/permission/dialogAdd.html',
            controller: 'InsertPermission',
            backdrop: 'static',
            size: 'lg'
        });
    };
    // call confirm delete
    $scope.dialogDelete = function (parameter) {
        var object = {
            ID: parameter
        }
        $http.post('/Home/deletePermission/', object).success(function (rs) {
            if (rs.error) {
                toaster.pop("error", "", rs.title, 1000, "");
            } else {
                toaster.pop("success", "", rs.title, 1000, "");
                $rootScope.reload();
            }
        });
    }
});
app.controller('ViewPermission', function ($scope, $http, $location, $uibModalInstance, $rootScope, parameter, toaster) {
    $scope.init = function () {
        // declare avaiable
        $scope.title = "Xem thông tin quyền."
        // getItem
        var callGetItem = function () {
            var object = {
                ID: parameter
            }
            $http.post('/Home/getItemPermission/', object).success(function (rs) {
                $scope.model = rs;
            });
        }
        // call getItem
        callGetItem();
    }
    // call init
    $scope.init();
    // function close dialog
    $scope.ok = function () {
        $uibModalInstance.close();
    };
    // function close dialog
    $scope.cancel = function () {
        $uibModalInstance.dismiss('cancel');
    };
});
app.controller('UpdatePermission', function ($scope, $http, $location, $uibModalInstance, $rootScope, parameter, toaster) {
    $scope.init = function () {
        // declare avaiable
        $scope.title = "Cập nhật quyền."
        // getItem
        var callGetItem = function () {
            var object = {
                ID: parameter
            }
            $http.post('/Home/getItemPermission/', object).success(function (rs) {
                $scope.model = rs;
            });
        }
        // call getItem
        callGetItem();
    }
    // call init
    $scope.init();
    // function close dialog
    $scope.ok = function () {
        $uibModalInstance.close();
    };
    // function close dialog
    $scope.cancel = function () {
        $uibModalInstance.dismiss('cancel');
    };
    // update permission
    $scope.submit = function () {
        $http.post('/Home/updatePermission/', $scope.model).success(function (rs) {
            if (rs.error) {
                toaster.pop("error", "", rs.title, 1000, "");
            } else {
                toaster.pop("success", "", rs.title, 1000, "");
                $rootScope.reload();
                $scope.cancel();
            }
        });
        //$rootScope.validateForm($scope.model, function (rs) {
        //    if (rs) {

        //    }
        //}, true);
    }
});
app.controller('InsertPermission', function ($scope, $http, $location, $uibModalInstance, $rootScope, parameter, toaster) {
    $scope.init = function () {
        // declare avaiable
        $scope.title = "Thêm mới quyền."
    }
    // call init
    $scope.init();
    // function close dialog
    $scope.cancel = function () {
        $uibModalInstance.dismiss('cancel');
    };
    // update permission
    $scope.submit = function () {
        $http.post('/Home/insertPermission/', $scope.model).success(function (rs) {
            if (rs.error) {
                toaster.pop("error", "", rs.title, 1000, "");
            } else {
                toaster.pop("success", "", rs.title, 1000, "");
                $rootScope.reload();
                $scope.cancel();
            }
        });
        //$rootScope.validateForm($scope.model, function (rs) {
        //    if (rs) {

        //    }
        //}, true);
    }
});