
var app = angular.module('InfinityLab');

app.controller('SampleInboxController', ['UserDetails', '$scope', '$http', '$location', '$rootScope', '$window', 'restService', 'ngDialog', '$state', '$timeout', '$filter',
    function (UserDetails, $scope, $http, $location, $rootScope, $window, restService, ngDialog, $state, $timeout, $filter) {
        $scope.SearchRef = restService.SearchReference;
        $scope.Company = [];
        $scope.Location = [];
        $scope.Results = [];
        $scope.igrid = null;
        restService.Get('ReferencelistService/AGENT', true).then(function (results) {
            $scope.Agents = results;
        });
        restService.Get('ReferencelistService/BUYINGCOMPANY', true).then(function (results) {
            $scope.Company = results;
        });
        restService.Get('ReferencelistService/ALLLOCATION', true).then(function (results) {
            $scope.Location = results;
        });
        $scope.ProductForms = restService.Get('ReferencelistService/MFGPRDFORM', true).then(function (results) {
            $scope.ProductForms = results;
        });
        $scope.ProductDivision = restService.Get('ReferencelistService/FULL-PRODUCTDIVISION', true).then(function (results) {
            $scope.ProductDivision = results;
        });
        $scope.SampleQualityList = restService.Get('ReferencelistService/MFGSAMPLEQUALITY', true).then(function (results) {
            $scope.SampleQualityList = results;
        });
        $scope.Employee = restService.Get('ReferencelistService/EMPLOY', true).then(function (results) {
            $scope.Employee = results;
        });
        $scope.Machines = restService.Get('ReferencelistService/FULL-MFGPRDMACHINE', true).then(function (results) {
            $scope.Machines = results;
        });
        $scope.FullStatusList = null;
        restService.Get('ReferencelistService/FULL-MFGSAMPLESTATUS', true).then(function (results) {
            results.push({ 'id': -1, 'name': 'Approved' });
            results.push({ 'id': -2, 'name': 'Deactivated' });
            results.push({ 'id': -3, 'name': 'Pending' });
            $scope.FullStatusList = (JSON.parse(JSON.stringify(results)));
            results.push({ 'id': 0, 'name': 'All' });
            $scope.StatusList = results;
        });
        $scope.Customers = null;
        restService.Get('ReferencelistService/RETAILCUSTOMER', true).then(function (results) {
            $scope.Customers = results;
        });
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
                url: "MfgsampleorderService/SearchSummary",
                data: srequest
            }).success(function (result) {
                $scope.Results = result;
                $scope.igrid.UpdateData($scope.Results);
            }).error(function (result) {
            });
        }

        $scope.PreLoad = function () {
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
                url: "MfgsampleorderService/SearchSummary",
                data: srequest
            }).success(function (result) {
                $scope.Results = result;
                $scope.ConfigGrid();
            }).error(function (result) {
            });
        };

        $scope.exporttoExcel = function (infygrid, filename, tableid) {
            var title = $rootScope.UserInfo.CompanyName;
            var subtitle = 'Sample Inbox'
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
                    'scope': $scope,
                    'status':status
                },
                width: '500px',
                controller: ['$scope', '$timeout', '$rootScope', function ($scope, $timeout, $rootScope) {
                    $scope.Parent = $scope.ngDialogData.scope;
                    $scope.ShowStatus = $scope.ngDialogData.status == 'status';
                    $scope.ShowReason = $scope.ngDialogData.status == 'deactivate';
                    
                    $scope.Selected = function () {
                        $scope.closeThisDialog({
                            'employeeid': $scope.employeeid,
                            'statusid': $scope.statusid,
                            'actionon': $scope.actionon,
                            'reason':$scope.reason
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
                            url: "MfgsampleorderService/UpdateStatus",
                            data: srequest
                        }).success(function (result) {
                            $rootScope.Toaster.Info(result.status);
                            $scope.PreLoad();
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
            igrid.AddSimpleColumn('designno', 'Design No', 100);
            igrid.AddFormatedColumn('entrydate', 'Date', 80, 'dd-MM-yyyy');
            igrid.AddSimpleColumn('duedays', 'Due', 50);
            
            //igrid.AddSimpleColumn('bookorderno', 'Booking No', 100);
            //igrid.AddFormatedColumn('bookingdate', 'Date', 80, 'dd-MM-yyyy');
            igrid.AddReferenceColumn('locationid', 'Location', 100, $scope.Location);
            igrid.AddReferenceColumn('prddivisionid', 'Divsion', 100, $scope.ProductDivision);
            igrid.AddReferenceColumn('customerid', 'Customer', 100, $scope.Customers);
            
            igrid.AddSimpleColumn('samplename', 'Sample Name', 100);
            igrid.AddFormatedColumn('price', 'Price', 80,'N3', 3);
            igrid.AddReferenceColumn('statusid', 'Status', 100, $scope.StatusList);
            igrid.AddReferenceColumn('agentid', 'Agent', 100, $scope.Agents);
            igrid.AddReferenceColumn('preparedby', 'Prepared', 100, $scope.Employee);
            igrid.AddReferenceColumn('approvedby', 'Approved', 100, $scope.Employee);
            column = igrid.AddCustomColumn('img', '', 50, function (item, col) {
                if (item.imageid && item.imageid != null && item.imageid != '')
                    return '<div style="height: 100%;display: flex;align-items: center;justify-content: center;"><img style="width:100%;height:100%" src="' + col.urlprefix + item.imageid + '"/></div>';
                return '';
            });
            column.urlprefix = 'File/Show/' + $scope.UserInfo.OrganizationID + '/';

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
                            $location.url('/mfgsampleorder?id=' + item.id);
                        }, 10);
                    }
                });
            });

            igrid.grid.onDblClick.subscribe(function (e) {
                var cell = igrid.grid.getCellFromEvent(e);
                var item = igrid.grid.getDataItem(cell.row);
                var command = igrid.grid.getColumns()[cell.cell].id;
                if (command == 'img') {
                    $scope.ShowImage('File/Show/' + $scope.UserInfo.OrganizationID + '/' + item.imageid);
                }
            });
            $scope.igrid.UpdateData($scope.Results);
        }

        $scope.ShowImage = function (image) {
            var dialog = ngDialog.open({
                template: 'ImageDialog',
                data: {
                    'scope': $scope,
                    'image': image
                },
                width: '500px',
                controller: ['$scope', '$timeout', '$rootScope', function ($scope, $timeout, $rootScope) {
                    $scope.imagepath = $scope.ngDialogData.image;

                    $scope.Selected = function () {
                        $scope.closeThisDialog();
                    };
                }],
                className: 'ngdialog-theme-plain'
            });
        }

        $scope.Create = function () {
            $location.url('/mfgsampleorder?id=0');
        }


        $scope.ApplicationSetting = function () {
            if ($rootScope.UserInfo &&
                $rootScope.UserInfo.appsettings &&
                $rootScope.UserInfo.appsettings != null && 
                $scope.Customers != null && 
                $scope.FullStatusList != null) {
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
