
var app = angular.module('InfinityLab');

app.controller('GatePassController', ['UserDetails', '$scope', '$http', '$location', '$rootScope', '$window', 'restService', 'ngDialog', '$state', '$timeout', '$filter', 'cntxService',
    function (UserDetails, $scope, $http, $location, $rootScope, $window, restService, ngDialog, $state, $timeout, $filter, cntxService) {
        $scope.SearchRef = restService.SearchReference;
        $scope.Location = [];
        $scope.Results = [];
        $scope.igrid = null;
        $scope.ShowCreate = false;
        $scope.LocalContext = cntxService.CreatePageContext('GatePass');
        restService.Get('ReferencelistService/TRANSPORT', true).then(function (results) {
            $scope.TransportList = results;
        });

        restService.Get('ReferencelistService/PRODUCTDIVISION', true).then(function (results) {
            $scope.ProductDivision = results;
        });

        restService.Get('ReferencelistService/CITY', true).then(function (results) {
            $scope.CityList = results;
        });

        restService.Get('ReferencelistService/EMPLOY', true).then(function (results) {
            $scope.EmployList = results;
        });

        restService.Get('ReferencelistService/UOM', true).then(function (results) {
            $scope.UOMList = results;
        });

        restService.Get('ReferencelistService/ALLLOCATION', true).then(function (results) {
            $scope.LocationList = results;
            $rootScope.BindLocation($scope, 'GatePass.locationid', $scope.PreLoad);
        });

        restService.Get('ReferencelistService/RETAILCUSTOMERLEDGER', true).then(function (results) {
            $scope.CustomerList = results;
        });

        restService.Get('ReferencelistService/SUPPLIERLEDGER', true).then(function (results) {
            $scope.VendorList = results;
        });

        $scope.MovementList = [
            { 'id': 'INWARDQUEUE', 'name': 'Inward Queue' },
            { 'id': 'OUTWARDQUEUE', 'name': 'Outward Queue' },
            { 'id': 'INWARD', 'name': 'Inward' },
            { 'id': 'OUTWARD', 'name': 'Outward' }];

        $scope.MovementCriteriaList = [
            { 'id': 'INWARD', 'name': 'Inward' },
            { 'id': 'OUTWARD', 'name': 'Outward' },
            { 'id': 'BOTH', 'name': 'Inward / Outward' },
            { 'id': 'PENDING', 'name': 'Pending' }];

        $scope.PackageTypes = [
            { 'id': '', 'name': 'None' },
            { 'id': 'BOM', 'name': 'BOM Purchase', 'code': 'Invoice No' },
            { 'id': 'PURCHASE', 'name': 'Purchase', 'code': 'Invoice No' },
            { 'id': 'JOBWORK', 'name': 'Jobwork', 'code': 'Job Work No' },
            { 'id': 'JOBORDER', 'name': 'Joborder', 'code': 'Job order No' },
            { 'id': 'WORKORDER', 'name': 'WorkOrder', 'code': 'Work Order No' },
            { 'id': 'BOMTRANSFER', 'name': 'BOM Transfer', 'code': 'Bundle Code' },
            { 'id': 'GOODSTRANSFER', 'name': 'Goods Transfer', 'code': 'Bundle Code' },
            { 'id': 'SERVICE', 'name': 'Service', 'code': 'DC Code' },
            { 'id': 'BILL', 'name': 'Bill', 'code': 'Bill No' }
        ];

        $scope.Clear = function () {
            $scope.GatePass = {
                "movement": "INWARD",
                "packagereferenceid":0,
                "locationid": $scope.DefaultLocationID,
                "packagetype": "PURCHASE",
                "packagecodename": "Invoice No",
                "packagedate": new Date(),
                "actionon": new Date(),
                "isoutward": false
            };
            $scope.GatePass._actionon = $filter('date')(new Date(), 'dd-MM-yyyy');
            $scope.GatePass._packagedate = $filter('date')(new Date(), 'dd-MM-yyyy');
        }

        $scope.Search = function () {
            request = {
                "Item": $scope.SearchRequest,
                "SecurityToken": "String content"
            }

            var srequest = JSON.stringify(request);

            $http({
                method: 'POST',
                url: "GatepassService/Search",
                data: srequest
            }).success(function (result) {
                $scope.Results = result;
                $scope.ConfigGrid();
            }).error(function (result) {
            });
        }

        $scope.PreLoad = function () {
            $scope.DefaultLocationID = $scope.GatePass.locationid;
            request = {
                "Item": {
                    locationid: $scope.GatePass.locationid,
                    showtoday: true
                },
                "SecurityToken": "String content"
            }
            var srequest = JSON.stringify(request);
            $http({
                method: 'POST',
                url: "GatepassService/Search",
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

        $scope.LoadValues = function () {
            if ($scope.GatePass.packagetype == 'JOBWORK' ||
                $scope.GatePass.packagetype == 'JOBORDER' ||
                $scope.GatePass.packagetype == 'WORKORDER' ||
                $scope.GatePass.packagetype == 'BOMTRANSFER' ||
                $scope.GatePass.packagetype == 'GOODSTRANSFER' ||
                $scope.GatePass.packagetype == 'BOM' ||
                $scope.GatePass.packagetype == 'PURCHASE' ||
                $scope.GatePass.packagetype == 'BILL') {

                $http({
                    method: 'POST',
                    url: "GatepassService/Defaults",
                    data: {
                        'item': {
                            'packagecode': $scope.GatePass.packagecode,
                            'packagetype': $scope.GatePass.packagetype,
                            'isoutward': $scope.GatePass.movement == 'OUTWARD'
                        }
                    }
                }).success(function (result) {
                    if (result.packagereferenceid == undefined || result.packagereferenceid == 0) {
                        $rootScope.Toaster.Info("Can't find the relevent records");
                        return;
                    }
                    $scope.GatePass.packagereferenceid = result.packagereferenceid;
                    $scope.GatePass.cityid = result.cityid;
                    if ($scope.GatePass.packagetype == 'BILL') {
                        $scope.GatePass.customerid = result.vendorid;
                    }
                    else {
                        $scope.GatePass.vendorid = result.vendorid;
                    }
                    if ($scope.GatePass.packagetype == 'PURCHASE' || $scope.GatePass.packagetype == 'BOM') {
                        $scope.setFocusinvoiceno();
                    }
                });
            }
        }

        $scope.ConfigGrid = function () {
            if ($scope.igrid && $scope.igrid != null)
                $scope.igrid.destroy();

            var igrid = new InfyGrid('gridContainer', '425px', '100%', true, true, true, 20);
            igrid.options.showHeaderRow = true;
            igrid.AddButtonColumn('Edit', '', 35, "<button type='button' style='width:30px;height:25px;margin-right:5px;line-height: 0;' value='Edit' class='btn btn-success btn-xs'><i class='fa fa-edit' style='font:normal normal normal 14px/1 FontAwesome'></i></button>");
            igrid.AddSimpleColumn('gatepassno', 'Gate Pass', 100);
            igrid.AddReferenceColumn('packagetype', 'Type', 80, $scope.PackageTypes);
            igrid.AddFormatedColumn('actionon', 'Date', 80, 'dd-MM-yyyy');
            igrid.AddReferenceColumn('inchargeid', 'Employee', 100, $scope.EmployList);
            igrid.AddFormatedColumn('packagedate', 'Package date', 80, 'dd-MM-yyyy');
            igrid.AddSimpleColumn('packagecode', 'Package No', 150);
            igrid.AddSimpleColumn('packagevalue', 'Package Value', 150);
            igrid.AddSimpleColumn('city', 'City', 100);
            igrid.AddSimpleColumn('transport', 'Transport', 100);
            igrid.AddSimpleColumn('driver', 'Driver', 100);
            igrid.AddSimpleColumn('drivercontactno', 'Contact No', 100);
            igrid.AddSimpleColumn('vehicleno', 'Vehicle No', 100);


            igrid.Initialize();
            $scope.igrid = igrid;

            igrid.grid.onClick.subscribe(function (e) {
                var cell = igrid.grid.getCellFromEvent(e);
                var item = igrid.grid.getDataItem(cell.row);
                var command = igrid.grid.getColumns()[cell.cell].id;
                $scope.$apply(function () {
                    if (command == 'Edit') {
                        $timeout(function () {
                            $scope.DisplayPass(item.id);
                        }, 10);
                    }
                });
            });
            $scope.igrid.UpdateData($scope.Results);
        }

        $scope.DisplayPass = function (id) {
            request = {
                "Item": {
                    id: id
                },
                "SecurityToken": "String content"
            }
            var srequest = JSON.stringify(request);
            $http({
                method: 'POST',
                url: "GatepassService/Search",
                data: srequest
            }).success(function (result) {
                $scope.ShowCreate = true;
                $scope.GatePass = result[0];
                $scope.GatePass.movement = $scope.GatePass.isoutward ? 'OUTWARD' : 'INWARD';


                if ($scope.GatePass.packagetype == 'BOMTRANSFER' || $scope.GatePass.packagetype == 'GOODSTRANSFER') {
                    var location = $scope.LocationList.find(function (item) {
                        return item.name == $scope.GatePass.vendor;
                    });
                    if (location && location != null) {
                        $scope.GatePass.vlocationid = location.id;
                    }

                } else if ($scope.GatePass.packagetype == 'BILL') {
                    var location = $scope.CustomerList.find(function (item) {
                        return item.name == $scope.GatePass.vendor;
                    });
                    if (location && location != null) {
                        $scope.GatePass.customerid = location.id;
                    }
                }
                else {
                    var location = $scope.VendorList.find(function (item) {
                        return item.name == $scope.GatePass.vendor;
                    });
                    if (location && location != null) {
                        $scope.GatePass.vendorid = location.id;
                    }
                }

                var transport = $scope.TransportList.find(function (item) {
                    return item.name == $scope.GatePass.transport;
                });
                if (transport && transport != null) {
                    $scope.GatePass.transportid = transport.id;
                }

                var transport = $scope.CityList.find(function (item) {
                    return item.name == $scope.GatePass.city;
                });
                if (transport && transport != null) {
                    $scope.GatePass.cityid = transport.id;
                }
            }).error(function (result) {
            });
        }


        $scope.AddSlide = function () {
            if ($scope.GatePass.attachments == undefined || $scope.GatePass.attachments == null)
                $scope.GatePass.attachments = [];
            $scope.GatePass.attachments.push($scope.Slide);
            $scope.Slide = { 'name': '', 'image': '' };
        };

        $scope.Save = function () {
            $scope.GatePass.actionby = $scope.UserInfo.EmployeeId;
            $scope.GatePass.actionon = new Date();
            $scope.GatePass.isoutward = $scope.GatePass.movement == 'OUTWARD';
            $scope.GatePass.vendor = '';

            if ($scope.GatePass.packagetype == 'BOMTRANSFER' || $scope.GatePass.packagetype == 'GOODSTRANSFER') {
                var location = $scope.LocationList.find(function (item) {
                    return item.id == $scope.GatePass.vlocationid;
                });
                if (location == undefined || location == null) {
                    $rootScope.Toaster.Info("Please select the location");
                    return;
                }
                $scope.GatePass.vendor = location.name;
            } else if ($scope.GatePass.packagetype == 'BILL') {


                var location = $scope.CustomerList.find(function (item) {
                    return item.id == $scope.GatePass.customerid;
                });
                if (location == undefined || location == null) {
                    $rootScope.Toaster.Info("Please select the customer");
                    return;
                }
                $scope.GatePass.vendor = location.name;

            }
            else {
                var location = $scope.VendorList.find(function (item) {
                    return item.id == $scope.GatePass.vendorid;
                });
                if (location == undefined || location == null) {
                    $rootScope.Toaster.Info("Please select the vendor");
                    return;
                }
                $scope.GatePass.vendor = location.name;
            }

            var transport = $scope.TransportList.find(function (item) {
                return item.id == $scope.GatePass.transportid;
            });
            if (transport == undefined || transport == null) {
                $rootScope.Toaster.Info("Please select the transport");
                return;
            }
            $scope.GatePass.transport = transport.name;

            var city = $scope.CityList.find(function (item) {
                return item.id == $scope.GatePass.cityid;
            });
            if (city == undefined || city == null) {
                $rootScope.Toaster.Info("Please select the city");
                return;
            }

            if ($scope.GatePass.packagereferenceid == 0) {
                if ($scope.GatePass.remarks == undefined ||
                    $scope.GatePass.remarks == null ||
                    $scope.GatePass.remarks == '') {
                    $rootScope.Toaster.Info("Remarks mandatory");
                    return;
                    }
            }

            $scope.GatePass.city = city.name;
            $scope.LocalContext.Post('$scope.GatePass', $scope.GatePass, false);

            var request = {
                "item": $scope.GatePass,
                "securitytoken": "String content"
            }
            var srequest = JSON.stringify(request);
            $http({
                method: 'POST',
                url: "GatepassService/Save",
                data: srequest
            }).success(function (result) {

                if (result != null && result.id > 0 &&
                    (result.ErrorMessage == 'undefined' || result.ErrorMessage == null || result.ErrorMessage == '')) {
                    $rootScope.Toaster.Info(result.gatepassno + " Saved Successfully.!");
                }
                else {
                    $rootScope.Toaster.Error(result.ErrorMessage);
                }

            }).error(function (result) {
            });
        }

        $scope.PackageTypeChanged = function () {
            $scope.GatePass.packagecodename = '';
            var packagetype = $scope.PackageTypes.find(function (item) {
                return item.id == $scope.GatePass.packagetype;
            });
            if (packagetype && packagetype != null)
                $scope.GatePass.packagecodename = packagetype.code;
        }


        $scope.ApplicationSetting = function () {
            if ($rootScope.UserInfo &&
                $rootScope.UserInfo.appsettings &&
                $rootScope.UserInfo.appsettings != null) {
                $scope.Clear();
                $scope.PreLoad();
                var value = $scope.LocalContext.Get('$scope.GatePass', false);
                if (value && value == null) {
                    $scope.GatePass = value;
                }
            }
            else {
                $timeout(function () {
                    $scope.ApplicationSetting();
                }, 10);
            }
        };
        $scope.ApplicationSetting();
    }]);
