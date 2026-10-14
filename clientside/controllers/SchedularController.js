
var app = angular.module('InfinityLab');

app.controller('SchedularController', ['UserDetails', '$scope', '$http', '$location', '$rootScope', '$window', 'restService', '$filter', 'Excel', '$timeout', '$stateParams',
    function (UserDetails, $scope, $http, $location, $rootScope, $window, restService, $filter, Excel, $timeout, $stateParams) {
        $rootScope.AccordionName = $location.path();
        $scope.SearchRef = restService.SearchReference;
        $scope.ConfiguredList = [];
        $scope.panelheight = { 'height': '500px' };
        $scope.listheight = { 'height': '300px' };

        $scope.TaskList = restService.Get('ScheduleService/TaskList', true).then(function (results) {
            $scope.TaskList = results;
            results.push({ 'name': 'AllSchedule', 'displayname': 'All Schedule' });
        });
        $scope.Company = restService.Get('ReferencelistService/BUYINGCOMPANY', true).then(function (results) {
            $scope.Company = results;
        });
        $scope.Location = restService.Get('ReferencelistService/LOCATION', true).then(function (results) {
            $scope.Location = results;
        });
        $scope.Section = restService.Get('ReferencelistService/DIVISION', true).then(function (results) {
            $scope.Section = results;
        });
        $scope.ProductList = restService.Get('ReferencelistService/PRODUCTS', true).then(function (results) {
            $scope.ProductList = results;
        });
        $scope.SupplierList = restService.Get('ReferencelistService/SUPPLIERLEDGER', true).then(function (results) {
            $scope.SupplierList = results;
        });
        $scope.Entity = null;
        $scope.HourList = [{ 'hour': '1', 'morning': false, 'evening': false }, { 'hour': '2', 'morning': false, 'evening': false }, { 'hour': '3', 'morning': false, 'evening': false },
            { 'hour': '4', 'morning': false, 'evening': false }, { 'hour': '5', 'morning': false, 'evening': false }, { 'hour': '6', 'morning': false, 'evening': false },
            { 'hour': '7', 'morning': false, 'evening': false }, { 'hour': '8', 'morning': false, 'evening': false }, { 'hour': '9', 'morning': false, 'evening': false },
            { 'hour': '10', 'morning': false, 'evening': false }, { 'hour': '11', 'morning': false, 'evening': false }, { 'hour': '12', 'morning': false, 'evening': false }];

        $scope.SalesStockSummaryModes = [{ 'id': 'Summary', 'name': 'Summary' }, { 'id': 'Product', 'name': 'Product' }, { 'id': 'Supplier', 'name': 'Supplier' }, { 'id': 'SupplierAndProduct', 'name': 'Supplier / Product' }];
        $scope.Monthlylist = [{ 'id': 'FirstDay', 'name': 'First Day' }, { 'id': 'LastDay', 'name': 'Last Day' }, { 'id': 'AllDay', 'name': 'All Day' }, { 'id': 'DayChange', 'name': 'Day Change' }];

        $scope.SendModes = [{ 'id': 'BODY', 'name': 'Body JSON' },
            { 'id': 'FORMDATA', 'name': 'Form Data' }];
        
        $scope.Outputlist = [{ 'id': 'SMS', 'name': 'SMS' },
            { 'id': 'PDFLink', 'name': 'PDF Link' },
            { 'id': 'Email', 'name': 'Email' },
            { 'id': 'EmailContent', 'name': 'Email (HTML)' },
            { 'id': 'PDFMail', 'name': 'PDF Attachment' },
            { 'id': 'ExcelMail', 'name': 'Excel Attachment' },
            { 'id': 'Whatsapp', 'name': 'Whats app Message' },
            { 'id': 'WebAPI', 'name': 'Web API' },
            { 'id': 'Growaasan', 'name': 'Grow AASAN' }];

        $scope.SaleReportModes = [
            { 'id': 0, 'name': 'Location' },
            { 'id': 1, 'name': 'Section' },
            { 'id': 2, 'name': 'Product' },
        ];

        $scope.Sourcelist = [{ 'id': 'Company', 'name': 'Company' }, { 'id': 'Customer', 'name': 'Customer' }, { 'id': 'Agent', 'name': 'Agent' }, { 'id': 'Supplier', 'name': 'Supplier' }, { 'id': 'Employee', 'name': 'Employee' }, { 'id': 'Custom', 'name': 'Custom' }];
        $scope.CurrentTask = {};
        $scope.TaskSelected = function (item) {
            $scope.CurrentTask = {};
            if (item.selected) {
                $scope.Entity = {};
                $scope.Search(item);
                $.each($scope.TaskList, function (k, v) {
                    if (v.name != item.name)
                        v.selected = false;
                    else {
                        $scope.CurrentTask = v;
                    }
                });
            }
        }

        $scope.New = function () {
            $scope.Entity = { 'description': $scope.SelectedTask.displayname, 'name': $scope.SelectedTask.name, 'isactive': true, 'version': 1, 'isactionbased': !$scope.SelectedTask.isschedulable };
            $scope.Entity.config = {};
            $scope.Entity.config.format = '';
            if ($scope.SelectedTask && $scope.SelectedTask.defaultjson && $scope.SelectedTask.defaultjson != null)
                $scope.Entity.config.format = $scope.SelectedTask.defaultjson
            $scope.Entity.config.daily = (JSON.parse(JSON.stringify($scope.HourList)));
        }

        $scope.Save = function () {

            if ($scope.SelectedTask != 'undefined' && $scope.SelectedTask != null) {
                $scope.Entity.name = $scope.SelectedTask.name;
                $scope.Entity.isactionbased = !$scope.SelectedTask.isschedulable;
            }
            $scope.Entity.isactive = true;
            if ($scope.Entity.id == 0)
                $scope.Entity.version = 1;
            request = {
                "Item": $scope.Entity,
                "securitytoken": "String content"
            }
            var srequest = JSON.stringify(request);
            $http({
                method: 'POST',
                url: "ScheduleService/Save",
                data: srequest
            }).success(function (result) {

                if (typeof ($scope.Entity.id) != 'undefined') {
                    $scope.Entity.version = result.version;
                    $rootScope.Toaster.Info("config updated");
                }
                else {
                    $scope.Entity.id = result.id;
                    $scope.Entity.version = result.version;
                    $scope.ConfiguredList.push(result);
                    $rootScope.Toaster.Info("New config added");
                }

                }).error(function (result) {
                    $rootScope.Toaster.Info("Error - " + result);
            });
        }

        $scope.SelectedTask = null;

        $scope.TaskSelected = function () {
            $scope.ConfiguredList = [];
            $scope.SelectedTask = null;
            if (typeof ($scope.selectedtask) != 'undefined') {
                $scope.SelectedTask = $scope.TaskList.find(function (item) {
                    return item.name == $scope.selectedtask;
                });
                $scope.SelectedTask.ParametersList = [];
                if ($scope.SelectedTask.Parameters != null && $scope.SelectedTask.Parameters.length > 0) {
                    $.each($scope.SelectedTask.Parameters, function (k, v) {
                        $scope.SelectedTask.ParametersList.push({ 'id': v, 'name': v });
                    });
                }
                $scope.Search(0);
            }
        }

        $scope.ManageSearchItem = function (type,item) {
            if (type == 'Edit') {
                $scope.Search(item.id, function (items) {
                    $scope.Entity = items[0];
                })
            } else if (type == 'Delete') {
                request = {
                    "Item": item,
                    "securitytoken": "String content"
                }
                var srequest = JSON.stringify(request);
                $http({
                    method: 'POST',
                    url: "ScheduleService/Delete",
                    data: srequest
                }).success(function (result) {
                    $scope.ConfiguredList.splice($scope.ConfiguredList.indexOf(item), 1);
                }).error(function (result) {
                });
            }
        }

        $scope.Import = function (data) {
            $scope.Entity.config = (JSON.parse(data));
            $scope.$apply();
        }

        $scope.Export = function () {
            var blob = new Blob([JSON.stringify($scope.Entity.config)],
                { type: "text/plain;charset=utf-8" });
            saveAs(blob, "Schedule.Config");
        }

        $timeout(function () {
            var available = (typeof ($rootScope.windowSize) != 'undefined' && typeof ($rootScope.windowSize.height) != 'undefined') ? $rootScope.windowSize.height : 0;
            if (available > 0) {
                $scope.panelheight.height = '' + (available - 150) + 'px';
                $scope.listheight.height = '' + (available - 250) + 'px';
            }
            $scope.setFocusfirst();
        }, 100);

        $scope.Search = function (id,callback) {

            request = {
                "Item": { 'name': $scope.selectedtask,id : id },
                "securitytoken": "String content"
            }
            var srequest = JSON.stringify(request);
            $http({
                method: 'POST',
                url: "ScheduleService/Search",
                data: srequest
            }).success(function (result) {
                if (typeof (callback) == 'undefined')
                    $scope.ConfiguredList = result;
                else
                    callback(result);
            }).error(function (result) {
            });
        }

        $scope.WeekList = [{ 'id': 1, 'name': 'SUNDAY' }, { 'id': 2, 'name': 'MONDAY' },
        { 'id': 3, 'name': 'TUESDAY' }, { 'id': 4, 'name': 'WEDNESDAY' },
        { 'id': 5, 'name': 'THURSDAY' }, { 'id': 6, 'name': 'FRIDAY' },
            { 'id': 7, 'name': 'SATURDAY' }];

    }]);