
var app = angular.module('InfinityLab');

app.controller('BOMInboxController', ['UserDetails', '$scope', '$http', '$location', '$rootScope', '$window', 'restService', 'ngDialog', '$state', '$timeout', '$filter',
    function (UserDetails, $scope, $http, $location, $rootScope, $window, restService, ngDialog, $state, $timeout, $filter) {
        $scope.SearchRef = restService.SearchReference;
        $scope.Company = [];
        $scope.Location = [];
        $scope.Results = [];
        $scope.igrid = null;
        restService.Get('ReferencelistService/BUYINGCOMPANY', true).then(function (results) {
            $scope.Company = results;
        });

        restService.Get('ReferencelistService/EMPLOY', true).then(function (results) {
            $scope.Employees = results;
        });

        restService.Get('ReferencelistService/ALLLOCATION', true).then(function (results) {
            $scope.Location = results;
        });


        $scope.StatusList = [
            { 'id': 0, 'name': 'Request' },
            { 'id': 1, 'name': 'Approved' },
            { 'id': 2, 'name': 'Rejected' },
            { 'id': 3, 'name': 'Ready' },
            { 'id': 4, 'name': 'Issued' },
            { 'id': 5, 'name': 'Received' },
            { 'id': 6, 'name': 'Consumed' }
        ];

        $scope.SearchRequest = {
            typeid: 0,
            addedfrom: null,
            supplierid: null,
            statusid: null,
            addedto: null
        };


        $scope.Search = function () {
            request = {
                "Item": $scope.SearchRequest,
                "SecurityToken": "String content"
            }

            var srequest = JSON.stringify(request);

            $http({
                method: 'POST',
                url: "BomrequestService/SearchInbox",
                data: srequest
            }).success(function (result) {
                $scope.Results = result;
                $scope.igrid.UpdateData($scope.Results);
            }).error(function (result) {
            });
        }

        $scope.PreLoad = function () {
            $scope.ismulticompany = $rootScope.UserInfo.appsettings.system_multicompany;
            $scope.ismultilocationentry = $rootScope.UserInfo.appsettings.system_showroompurchase;
            request = {
                "Item": {
                    typeid: 0,
                    addedfrom: null,
                    addedto: null,
                    showallopen: true
                },
                "SecurityToken": "String content"
            }
            var srequest = JSON.stringify(request);
            $http({
                method: 'POST',
                url: "BomrequestService/SearchInbox",
                data: srequest
            }).success(function (result) {
                $scope.Results = result;
                $scope.ConfigGrid();
            }).error(function (result) {
            });
        };

        $scope.exporttoExcel = function (infygrid, filename, tableid) {
            var title = $rootScope.UserInfo.CompanyName;
            var subtitle = 'WAREHOUSE DASHBOARD - INFORMATION'
            $scope.exportHref = infygrid.ExportExcel(infygrid.grid, title, subtitle);
            if ($scope.exportHref != null) {
                $timeout(function () {
                    var link = document.createElement('a');
                    link.download = filename + ".xls";
                    link.href = $scope.exportHref;
                    document.body.appendChild(link);
                    link.click();
                    link.remove();
                }, 100);
            }
        }

        $scope.UpdateStatus = function (status) {
            var dialog = ngDialog.open({
                template: 'ChangeStatusDialog',
                data: {
                    'scope': $scope
                },
                width: '500px',
                controller: ['$scope', '$timeout', '$rootScope', function ($scope, $timeout, $rootScope) {
                    $scope.Parent = $scope.ngDialogData.scope;
                    $scope.Selected = function () {
                        $scope.closeThisDialog({
                            'employeeid': $scope.employeeid,
                            'statuson': $scope.statuson
                        });
                    };
                }],
                className: 'ngdialog-theme-plain'
            });
            dialog.closePromise.then(function (data) {
                {
                    if (typeof (data.value) != 'undefined' && data.value != null &&
                        data.value != '$escape' && data.value != '$document') {
                        var request = data.value;
                        request.ids = [];
                        request.status = status;
                        var items = $scope.igrid.GetSelectedRows();
                        if (items.length == 0)
                            return;
                        $.each(items, function (k, v) {
                            request.ids.push(v.id);
                        });

                        request = {
                            "Item": request,
                            "SecurityToken": "String content"
                        }

                        var srequest = JSON.stringify(request);

                        $http({
                            method: 'POST',
                            url: "BomrequestService/UpdateStatus",
                            data: srequest
                        }).success(function (result) {
                            var items = $scope.igrid.GetSelectedRows();
                            var statusid = 0;
                            if (status == 'approve') statusid = 1;
                            if (status == 'reject') statusid = 2;
                            if (status == 'issued') statusid = 4;
                            if (status == 'receive') statusid = 5;
                            $.each(items, function (k, v) {
                                v.status = statusid;
                            });
                            $scope.igrid.UpdateData($scope.Results);
                        }).error(function (result) {
                        });
                    }
                }
            });
        }

        $scope.ConfigGrid = function () {
            if ($scope.igrid && $scope.igrid != null)
                $scope.igrid.destroy();

            var igrid = new InfyGrid('gridContainer', '425px', '100%', true, true, true, 20);
            igrid.options.showHeaderRow = true;

            igrid.AddSimpleColumn('entryno', 'Entry No', 100);
            igrid.AddFormatedColumn('entrydate', 'Date', 80, 'dd-MM-yyyy');
            igrid.AddSimpleColumn('fromlocation', 'From', 150);
            igrid.AddSimpleColumn('tolocation', 'To', 150);
            var column = igrid.AddCustomColumn('Work order', 'Work order', 150, function (item) {
                return item.workorderno + ' @ ' + $filter('date')(item.wodate, 'dd-MMM-yyyy');
            });
            var column = igrid.AddCustomColumn('Job Order', 'Job Order', 150, function (item) {
                if (item.joborderno && item.joborderno != null &&
                    item.joborderno != '')
                    return item.joborderno + ' @ ' + $filter('date')(item.joborderdate, 'dd-MMM-yyyy');
                return '';
            });
            igrid.AddSimpleColumn('stagename', 'Stage', 100);
            igrid.AddSimpleColumn('approvedemp', 'Approved By', 100);
            igrid.AddReferenceColumn('status', 'Status', 100, $scope.StatusList);
            igrid.AddButtonColumn('Edit', '', 35, "<button type='button' style='width:30px;height:25px;margin-right:5px;line-height: 0;' value='Edit' class='btn btn-success btn-xs'><i class='fa fa-edit' style='font:normal normal normal 14px/1 FontAwesome'></i></button>");
            igrid.Initialize();
            $scope.igrid = igrid;

            igrid.grid.onClick.subscribe(function (e) {
                var cell = igrid.grid.getCellFromEvent(e);
                var item = igrid.grid.getDataItem(cell.row);
                var command = igrid.grid.getColumns()[cell.cell].id;
                $scope.$apply(function () {
                    if (command == 'Edit') {
                        $timeout(function () {
                            $location.url('/BOMRequest?requestid=' + item.id);
                        }, 10);
                    }
                });
            });
            $scope.igrid.UpdateData($scope.Results);
        }

        $scope.Bundle = function () {
            var items = $scope.igrid.GetSelectedRows();
            if (items.length == 1 && items[0].mode == 6) {
                $location.url('/BOMOutward?requestid=' + items[0].id);
            }
        }

        $scope.Create = function () {
            $location.url('/BOMRequest?transfer=true');
        }

        $scope.Prepare = function () {
            var items = $scope.igrid.GetSelectedRows();
            if (items.length == 1) {
                $location.url('/BOMConsumption?requestid=' + items[0].id);
            }
        }


        $scope.ApplicationSetting = function () {
            if ($rootScope.UserInfo &&
                $rootScope.UserInfo.appsettings &&
                $rootScope.UserInfo.appsettings != null) {
                $scope.PreLoad();
            }
            else {
                $timeout(function () {
                    $scope.ApplicationSetting();
                }, 10);
            }
        };
        $scope.ApplicationSetting();
    }]);
