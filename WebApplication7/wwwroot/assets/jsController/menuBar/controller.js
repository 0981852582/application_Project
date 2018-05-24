var app = angular.module('rootApplication', ['ui.bootstrap', 'ui.router', 'oc.lazyLoad', 'toaster']);
//jsController
app.controller('mainMenubar', function ($scope, $rootScope, $http, $uibModal, $timeout, toaster) {
    $rootScope.url = {
        add: '/menuBar/insertmenuBar/',
        edit: '/menuBar/updatemenuBar/',
        delete: '/menuBar/deletemenuBar/',
        getItem: '/menuBar/getItemmenuBar/',
        getListItem: '/menuBar/getListmenuBar/'
    }
    $scope.init = function () {
        // required
        $rootScope.aliasPermission = 'menuBar';
        initData($rootScope, $http);
        $rootScope.checkAccessMenuBar($rootScope.aliasPermission, function (rs) {
            if (!rs) {
                window.location.href = '/Account/Login';
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
                $http.post($rootScope.url.getListItem, this).success(function (rs) {
                    if (!rs.error) {
                        cursor.totalItem = rs.result.totalItem;
                        cursor.results = rs.result.results;
                        cursor.fromRow = rs.result.fromRow;
                        cursor.endRow = rs.result.endRow;
                        $timeout(() => {
                            UnBlockUI("body");
                        }, 1);
                    }
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
            templateUrl: '../assets/jsController/menuBar/dialogView.html',
            controller: 'ViewMenuBar',
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
            templateUrl: '../assets/jsController/menuBar/dialogUpdate.html',
            controller: 'UpdateMenuBar',
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
            templateUrl: '../assets/jsController/menuBar/dialogAdd.html',
            controller: 'InsertMenuBar',
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
            Title: 'title',
            rule: {
                Required: true,
                Maxlength: 255
            },
            message: {
                Required: 'Tiêu đề không được để trống.',
                Maxlength: 'Tiêu đề không được lớn hơn 255 ký tự.'
            },
            Place: 'col-lg-4'
        },
        {
            Title: 'url',
            rule: {
                Required: true,
            },
            message: {
                Required: 'Url không được để trống.'
            },
            Place: 'col-lg-4'
        }
    ];
});
app.controller('ViewMenuBar', function ($scope, $http, $location, $uibModalInstance, $rootScope, parameter, toaster) {
    $scope.init = function () {
        // declare avaiable
        $scope.title = "Xem thông tin quyền."
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
app.controller('UpdateMenuBar', function ($scope, $http, $location, $uibModalInstance, $rootScope, parameter, toaster) {
    $scope.init = function () {
        // declare avaiable
        $scope.title = "Cập nhật quyền."
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
    // update menuBar
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
app.controller('InsertMenuBar', function ($scope, $http, $location, $uibModalInstance, $rootScope, toaster) {
    $scope.init = function () {
        // declare avaiable
        $scope.title = "Thêm mới quyền."
        $scope.model = {};
    }
    // call init
    $scope.init();
    // function close dialog
    $scope.cancel = function () {
        $uibModalInstance.dismiss('cancel');
    };
    // update menuBar
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