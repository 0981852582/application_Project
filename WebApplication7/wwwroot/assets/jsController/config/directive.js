app.filter('fdate', [
    '$filter', function ($filter) {
        return function (input, f) {
            if (input && input.toString().indexOf('Date') > -1) {
                return moment(input).format(f);
            } if (input && input.toString().indexOf('T') > -1) {
                return moment(input).format(f);
            } else return input;
        };
    }
]);
var convertDateFull = function (value) {
    return (new Date(value).getDate() < 10 ? '0' : '') + '' + new Date(value).getDate() + '/' + ((new Date(value).getMonth() + 1) < 10 ? '0' : '') + '' + (new Date(value).getMonth() + 1) + '/' + new Date(value).getFullYear() + ' ' + (new Date(value).getHours() < 10 ? '0' : '') + '' + new Date(value).getHours() + ':' + (new Date(value).getMinutes() < 10 ? '0' : '') + '' + new Date(value).getMinutes();
}
var convertOnlyDate = function (value) {
    return (new Date(value).getDate() < 10 ? '0' : '') + '' + new Date(value).getDate() + '/' + ((new Date(value).getMonth() + 1) < 10 ? '0' : '') + '' + (new Date(value).getMonth() + 1) + '/' + new Date(value).getFullYear();
}
app.directive('datetimePickerFull', ['$filter', function () {
    return {
        require: '?ngModel',
        link: function (scope, elem, attrs, ctrl) {
            $(elem).datetimepicker({
                timepicker: true,
                step: 5,
                format: 'd/m/Y H:i'
            });
            ctrl.$formatters.unshift(function (viewValue) {
                if (viewValue != undefined) {
                    if (viewValue.indexOf('/Date') != -1) {
                        viewValue = parseFloat(viewValue.substr(6, 13));
                    }
                    return convertDateFull(viewValue);
                }
            });
            ctrl.$parsers.unshift(function (viewValue) {
                if (viewValue != undefined && viewValue != '') {
                    try {
                        var hours = viewValue.split(' ');
                        var split = hours[0].split('/');
                        var result = '/Date(' + (new Date(split[2] + '-' + split[1] + '-' + split[0] + ' ' + hours[1]).getTime()) + ')/';
                        return result;
                    } catch (ex) {
                        return '';
                    }
                }
            });
        }
    };
}]);
app.directive('datetimePickerOnlyDate', ['$filter', function () {
    return {
        require: '?ngModel',
        link: function (scope, elem, attrs, ctrl) {
            $(elem).datetimepicker({
                timepicker: false,
                step: 5,
                format: 'd/m/Y'
            });
            ctrl.$formatters.unshift(function (viewValue) {
                if (viewValue != undefined) {
                    if (viewValue.indexOf('/Date') != -1) {
                        viewValue = parseFloat(viewValue.substr(6, 13));
                    }
                    return convertOnlyDate(viewValue);
                }
            });
            ctrl.$parsers.unshift(function (viewValue) {
                if (viewValue != undefined && viewValue != '') {
                    try {
                        var split = viewValue.split('/');
                        var result = '/Date(' + (new Date(split[2] + '-' + split[1] + '-' + split[0]).getTime()) + ')/';
                        return result;
                    } catch (ex) {
                        return '';
                    }
                }
            });
        }
    };
}]);
// validateForm
var $scope;
var initData = function (rootScope,$http) {
    if ($scope == undefined)
        $scope = rootScope;
    // config permisstion
    $scope.checkPermissionPage = function (url, callback) {
        $http.get('/permission/getPermissionMenuBar?urlPage=' + url).success(function (rs) {
            if (rs.error) {
                toaster.pop("error", "", rs.title, 1000, "");
                return callback(false);
            } else {
                return callback(rs);
            }
        });
    }
    $scope.checkPermissionPage($scope.aliasPermission, function (rs) {
        if (rs) {
            $scope.permission = rs;
            for (var i = 0; i < $scope.permission.result.length; i++) {
                $scope.permission[$scope.permission.result[i].typePermission] = $scope.permission.result[i].status;
            }
        }
    });
    // validate form
    var convert = function (get) {
        for (var i = 0; i < get.length; i++) {
            get[i].status = {};
            if (get[i].Data[get[i].Title] == undefined) get[i].Data[get[i].Title] = null;
            if (get[i].Data[get[i].Title] == "") get[i].Data[get[i].Title] = null;
            if (get[i].Place == undefined) get[i].Place = 'col-md-4';
        }
    }
    var review = function (key, value, status) {
        if (key == 'Required' && status) {
            if (value == null) return false;
        }
        if (key == 'IsNumber' && status) {
            var checkIs = true;
            if (value == null) return false;
            else {
                for (var i = 0; i < value.length; i++) {
                    if (parseInt(value[i]).toString() == "NaN") {
                        checkIs = false;
                        break;
                    }
                }
            }
            return checkIs;
        }
        if (key == "Min" && parseFloat(value).toString() != "NaN") {
            if (value < parseFloat(status)) return false;
        }
        if (key == "Max" && parseFloat(value).toString() != "NaN") {
            if (value > parseFloat(status)) return false;
        }
        if (key == "Maxlength" && parseInt(value).toString() != "NaN") {
            if (value.length > parseInt(status)) return false;
        }
        if (key == "Special" && status) {
            if (/[^a-zA-Z0-9\-\/]/.test(value))
                return false;
        }
        return true;
    }
    var display = function (get) {
        var save = get;
        for (var i = 0; i < get.length; i++) {
            var each = Object.keys(get[i].status);
            for (var j = 0; j < each.length; j++) {
                var lastClass = "JD3T2QH36RX7W2W7R3XTDVRPQ";
                if (get[i].status[each[j]] == false) {
                    $("." + get[i].Title + lastClass).remove();
                    $("[name='" + get[i].Title + "']")
                        .parents("." + get[i].Place)
                        .addClass("has-error")
                        .append('<span class="help-block ' + get[i].Title + lastClass + '">' + get[i].message[each[j]] + '</span>');
                    $("[name='" + get[i].Title + "']")
                        .parents('.' + get[i].Place)
                        .prev('label')
                        .css({ 'color': '#e73d4a' });

                } else {
                    $("[name='" + get[i].Title + "']").parents('.' + get[i].Place).removeClass('has-error');
                    $("." + get[i].Title + lastClass + "").remove();
                    $("[name='" + get[i].Title + "']")
                        .parents('.' + get[i].Place)
                        .prev('label')
                        .css({ 'color': 'black' });
                }
            }
        }
    }
    var validate = function (get, callback) {
        var call = true;
        convert(get);
        for (var i = 0; i < get.length; i++) {
            get[i].status = {};
            each = Object.keys(get[i].rule);
            for (var j = 0; j < each.length; j++) {
                if (get[i].rule[each[j]]) {
                    eachRule = Object.keys(get[i].status);
                    var checked = true;
                    for (var t = 0; t < eachRule.length; t++) {
                        if (get[i].status[eachRule[t]] == false) checked = false;
                    }
                    if (checked) {
                        get[i].status[each[j]] = review(each[j], get[i].Data[get[i].Title], get[i].rule[each[j]]);
                        if (!get[i].status[each[j]]) call = false;
                    }
                }

            }
        }
        display(get);
        return callback(call);
    }
    $scope.$watch('dataForm', function (newvalue, oldvalue) {
        if (newvalue != undefined) {
            $scope.validateForm($scope.dataForm, function (rs) { }, $scope.statusForm);
        }
    }, true);
    $scope.validateForm = function (inputData, callback, status) {
        $scope.dataForm = inputData;
        $scope.statusForm = status;
        if (!status) return false;
        var call = true;
        // directive for object validate
        for (var i = 0; i < $scope.validationOptions.length; i++) {
            $scope.validationOptions[i].Data = inputData;
        }
        for (var i = 0; i < $scope.validationOptions.length; i++) {
            $("[name='" + $scope.validationOptions[i].Title + "']").addClass("jfdkjfkdjkjeereavc");
        }
        validate($scope.validationOptions, function (rs) {
            call = rs;
        });
        return callback(call);
    }
    $scope.validateForm();
    //caculator total
    $scope.CaculatorTotal = function (arr, object) {
        if (arr != undefined && arr != null) {
            if (arr.length > 0) {
                var total = 0;
                var str = object.split('.');
                try {
                    for (var i = 0; i < arr.length; i++) {
                        var value = undefined;
                        for (var j = 0; j < str.length; j++) {
                            if (value == undefined)
                                value = arr[i][str[j]];
                            else if (value != undefined)
                                value = value[str[j]];
                        }
                        total += (value != null && value != undefined && value != '' ? parseFloat(value) : 0);
                    }
                    return spassAresSlug(Round(total.toString()).toString(), true);
                } catch (e) {
                    alert('dữ liệu đầu vào không đúng khi tính tổng.');
                }
            }
        }
        return 0;
    }
};