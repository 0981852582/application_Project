var app = angular.module('rootApplication', ['ui.bootstrap', 'ui.router', 'oc.lazyLoad', 'toaster']);
//jsController
app.controller('mainAccount', function ($scope, $rootScope, $http, $uibModal, $timeout, toaster) {
    $rootScope.url = {
        add: '/Account/insertAccount/',
        edit: '/Account/updateAccount/',
        delete: '/Account/deleteAccount/',
        getItem: '/Account/getItemAccount/',
        getListItem: '/Account/getListAccount/'
    }
    
    $scope.init = function () {
        $scope.title = "Quản lý tài khoản.";
        // required
        $rootScope.aliasPermission = 'account';
        initData($rootScope, $http);
        $rootScope.checkAccessMenuBar($rootScope.aliasPermission, function (rs) {
            if (!rs) {
                window.location.href = '';
            }
        });
        //required
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
                numberPage: 2,
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
                $http.post($rootScope.url.getListItem, this).success(function (rs) {
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
            templateUrl: '../assets/jsController/Account/dialogView.html',
            controller: 'ViewAccount',
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
            templateUrl: '../assets/jsController/Account/dialogUpdate.html',
            controller: 'UpdateAccount',
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
            templateUrl: '../assets/jsController/Account/dialogAdd.html',
            controller: 'InsertAccount',
            backdrop: 'static',
            size: 'lg'
        });
    };
    // call confirm delete
    $scope.dialogDelete = function (parameter,title) {
        Comfirm("Bạn có chắc chắn muốn xóa quyền [ " + title + " ] ?", function (rs) {
            if (rs) {
                var object = {
                    ID: parameter
                }
                $http.post($rootScope.url.delete, object).success(function (rs) {
                    if (rs.error) {
                        toaster.pop("error", "", rs.title, 1000, "");
                    } else {
                        toaster.pop("success", "", rs.title, 1000, "");
                        $rootScope.reload();
                    }
                });
            }
        });
    }
    // validate
    // object validate form
    $rootScope.validationOptions = [
        {
            Title: 'username',
            rule: {
                Required: true,
                Maxlength: 25,
                Special : true
            },
            message: {
                Required: 'Tài khoản không được để trống.',
                Maxlength: 'Tài khoản không được lớn hơn 25 ký tự.',
                Special: 'Tài khoản không được có kí tự đặc biệt.'
            },
            Place: 'col-lg-4'
        },
        {
            Title: 'password',
            rule: {
                Required: true,
                Maxlength: 25
            },
            message: {
                Required: 'Mật khẩu không được để trống.',
                Maxlength: 'Mật khẩu không được lớn hơn 25 ký tự.'
            },
            Place: 'col-lg-4'
        },
        {
            Title: 'fullName',
            rule: {
                Required: true,
                Maxlength: 25
            },
            message: {
                Required: 'Họ tên không được để trống.',
                Maxlength: 'Họ tên không được lớn hơn 25 ký tự.'
            },
            Place: 'col-lg-4'
        }
    ];
});
app.controller('ViewAccount', function ($scope, $http, $location, $uibModalInstance, $rootScope, parameter, toaster) {
    $scope.init = function () {
        // declare avaiable
        $scope.title = "Xem thông tin tài khoản."
        // getItem
        var callGetItem = function () {
            var object = {
                ID: parameter
            }
            $http.post($rootScope.url.getItem, object).success(function (rs) {
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
app.controller('UpdateAccount', function ($scope, $http, $location, $uibModalInstance, $rootScope, parameter, toaster) {
    $scope.init = function () {
        // declare avaiable
        $scope.title = "Cập nhật thông tin tài khoản."
        $scope.model = {};
        // getItem
        var callGetItem = function () {
            var object = {
                ID: parameter
            }
            $http.post($rootScope.url.getItem, object).success(function (rs) {
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
    // update Account
    $scope.submit = function () {
        $rootScope.validateForm($scope.model, function (rs) {
            if (rs) {
                $http.post($rootScope.url.edit, $scope.model).success(function (rs) {
                    if (rs.error) {
                        toaster.pop("error", "", rs.title, 1000, "");
                    } else {
                        toaster.pop("success", "", rs.title, 1000, "");
                        $rootScope.reload();
                        $scope.cancel();
                    }
                });
            }
        }, true);
    }
});
app.controller('InsertAccount', function ($scope, $http, $location, $uibModalInstance, $rootScope, toaster) {
    $scope.init = function () {
        // declare avaiable
        $scope.title = "Thêm mới thông tin tài khoản."
        $scope.model = {};
    }
    // call init
    $scope.init();
    // function close dialog
    $scope.cancel = function () {
        $uibModalInstance.dismiss('cancel');
    };
    // update Account
    $scope.submit = function () {
        $rootScope.validateForm($scope.model, function (rs) {
            if (rs) {
                $http.post($rootScope.url.add, $scope.model).success(function (rs) {
                    if (rs.error) {
                        toaster.pop("error", "", rs.title, 1000, "");
                    } else {
                        toaster.pop("success", "", rs.title, 1000, "");
                        $rootScope.reload();
                        $scope.cancel();
                    }
                });
            }
        }, true);

    }
});