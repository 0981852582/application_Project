var app = angular.module('rootApplication', ['ui.bootstrap', 'ui.router', 'oc.lazyLoad', 'toaster']);
//jsController
app.controller('mainPermisstion', function ($scope, $rootScope, $http, $uibModal, $timeout, toaster) {
    $rootScope.url = {
        add: '/Permission/insertPermission/',
        edit: '/Permission/updatePermission/',
        delete: '/Permission/deletePermission/',
        getItem: '/Permission/getItemPermission/',
        getListItem: '/Permission/getListPermission/'
    };
    $scope.init = function () {
        // required
        $rootScope.aliasPermission = 'permission';
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
            templateUrl: '../assets/jsController/permission/dialogView.html',
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
            templateUrl: '../assets/jsController/permission/dialogUpdate.html',
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
            templateUrl: '../assets/jsController/permission/dialogAdd.html',
            controller: 'InsertPermission',
            backdrop: 'static',
            size: 'lg'
        });
    };
    // call confirm delete
    $scope.dialogDelete = function (parameter, title) {
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
    // call dialog view
    $scope.dialogPermission = function (data) {
        /*begin modal*/
        var modalInstance = $uibModal.open({
            templateUrl: '../assets/jsController/permission/dialogPermission.html',
            controller: 'ApplyPermission',
            backdrop: 'static',
            resolve: {
                parameter: function () {
                    return data;
                }
            }
        });
    };
    // validate
    // object validate form
    $rootScope.validationOptions = [
        {
            Title: 'title',
            rule: {
                Required: true,
                Maxlength: 25
            },
            message: {
                Required: 'Tiêu đề không được để trống.',
                Maxlength: 'Mã nhà cung cấp không được lớn hơn 25 ký tự.'
            },
            Place: 'col-lg-4'
        },
        {
            Title: 'permissionCode',
            rule: {
                Required: true,
                Maxlength: 8,
                Special: true
            },
            message: {
                Required: 'Mã quyền không được để trống.',
                Maxlength: 'Mã quyền không được lớn hơn 8 ký tự.',
                Special: 'Mã quyền không được có ký tự đặc biệt.'
            },
            Place: 'col-lg-4'
        }
    ];
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
app.controller('UpdatePermission', function ($scope, $http, $location, $uibModalInstance, $rootScope, parameter, toaster) {
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
    // update permission
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
app.controller('InsertPermission', function ($scope, $http, $location, $uibModalInstance, $rootScope, toaster) {
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
    // update permission
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
app.controller('ApplyPermission', function ($scope, $http, $location, $uibModalInstance, $rootScope, parameter, toaster) {
    $scope.init = function () {
        // declare avaiable
        $scope.title = "Phân quyền chức năng cho quyền [ " + parameter.title + " ]."
        // get menuBar
        $http.post('/menuBar/getAllListMenuBar/', ).success(function (rs) {
            if (rs.error) {
                toaster.pop("error", "", rs.title, 1000, "");
            } else {
                $scope.listAllMenuBar = rs.result;
            }
        });
    }
    // call init
    $scope.init();
    // change select
    $scope.changeChoice = function () {
        let item = $scope.listAllMenuBar.find(x => x.id == $scope.model.choice)
        let object = {
            urlPage: item.urlCode,
            permissionCode: parameter.permissionCode
        }
        $scope.urlCode = item.urlCode;
        $http.post('/permission/getPermissionMenuBarConfig/', object).success(function (rs) {
            if (rs.error) {
                toaster.pop("error", "", rs.title, 1000, "");
            } else {
                $scope.listAllPermission= rs.result;
            }
        });
    }
    $scope.submit = function () {
        var object = {
            UrlCode: $scope.urlCode,
            PermissionCode: parameter.permissionCode,
            listPermission: $scope.listAllPermission
        }
        $http.post('/permission/ApplyPermission/', object).success(function (rs) {
            if (rs.error) {
                toaster.pop("error", "", rs.title, 1000, "");
            } else {
                toaster.pop("success", "", rs.title, 1000, "");
            }
        });
    }
    // function close dialog
    $scope.ok = function () {
        $uibModalInstance.close();
    };
    // function close dialog
    $scope.cancel = function () {
        $uibModalInstance.dismiss('cancel');
    };
});