
var app = angular.module('InfinityLab');

app.controller('ROLStoreViewController', ['UserDetails', '$scope', '$http', '$location', '$rootScope', '$window', 'restService', '$timeout', 'ngDialog', 'cntxService','$state',
    function (UserDetails, $scope, $http, $location, $rootScope, $window, restService, $timeout, ngDialog, cntxService, $state) {
        $scope.Navigation = $rootScope.GetNavigation();
        $rootScope.AccordionName = $location.path();
        $scope.SearchRef = restService.SearchReference;
        $scope.SearchResult = [];
        $scope.SearchCondition = {
            name: "",
            contactperson: "",
            locationid: ""
        }
        $scope.BuyingCompany = restService.Get('ReferencelistService/COMPANY', true).then(function (results) {
            $scope.BuyingCompany = results;
        });
        
        $scope.Locations = restService.Get('ReferencelistService/MYLOCATION', true).then(function (results) {
            $scope.Locations = results;
        });
        $scope.Supplier = restService.Get('ReferencelistService/SUPPLIER', true).then(function (results) {
            $scope.Supplier = results;
        });

        $scope.DetailedResult = [];

        $scope.LocationChanged = function () {
            $scope.SearchResult = [];
            if (typeof ($scope.SearchCondition.locationid) != 'undefined') {
                var tolocation = $scope.Locations.find(function (item) {
                    return item.id == $scope.SearchCondition.locationid;
                });
                $scope.SearchCondition.iswarehouse = tolocation.additionalinfo &&
                    tolocation.additionalinfo != null &&
                    tolocation.additionalinfo.iswarehouse;
                $scope.SearchCondition.companyid = (tolocation.additionalinfo &&
                    tolocation.additionalinfo != null) ?
                    tolocation.additionalinfo.parentid : 0;
            }
        }

        $scope.SearchROLStoreView = function () {

            request = {
                "Item": $scope.SearchCondition,
                "securitytoken": "String content"
            }

            var srequest = JSON.stringify(request);

            $http({
                method: 'POST',
                url: "WarehouseReportService/ROLStoreView",
                data: srequest
            }).success(function (result) {

                $scope.SearchResult = result;
                $scope.ROLStoreViewConfig();
            }).error(function (result) {
            });

        }


        $scope.ROLStoreViewConfig = function () {
            if ($scope.igrid != null)
                $scope.igrid.destroy();

            var igrid = new InfyGrid('gridContainer', '450px', '100%', true, false, true, 0);
            igrid.options.showHeaderRow = true;

            igrid.AddReferenceColumn('supplierid', 'SUPPLIER', 180, $scope.Supplier);
            igrid.AddSimpleColumn('lrno', 'LR NO', 120);
            igrid.AddFormatedColumn('lrdate', 'LR DATE', 120, 'dd-MM-yyyy');
            igrid.AddSimpleColumn('invoiceno', 'INV NO', 120);
            igrid.AddFormatedColumn('invoicedate', 'INV DATE', 120, 'dd-MM-yyyy');
            igrid.AddSimpleColumn('productname', 'PRODUCT NAME', 180);

            igrid.AddSimpleColumn('qty', 'STOCK QTY', 100);
            //igrid.AddSimpleColumn('pendingdays', 'PENDING DAYS', 120);

            var percentageValueColumn = igrid.AddCustomColumn('pendingdays', 'Days', 100, function (item, col) {

                var days = item.pendingdays;
                var parts = [];
                if (days > 365) {
                    var year = Math.floor(days / 365);
                    parts.push(year.toString() + " years");
                    days -= (365 * year);
                }
                if (days > 30) {
                    var year = Math.floor(days / 30);
                    parts.push(year.toString() + " months");
                    days -= (30 * year);
                }
                if (parts.length <= 1 && days > 0) {
                    parts.push(days.toString() + " days");
                }
                return parts.join(' ');

            });

            percentageValueColumn.sortable = true;
            percentageValueColumn.getValue = function (item, col) {
                return col.pendingdays;
            }

            igrid.AddButtonColumn('action', 'Action', 90, 'VIEW', function (e, args) {
                $scope.$apply(function () {
                    $scope.ViewData(args.item);
                });
                return true;
            });
            igrid.options.autoEdit = true;
            igrid.Initialize();
            $scope.igrid = igrid;
            $scope.igrid.UpdateData($scope.SearchResult);

        }

        $scope.ViewData = function (item) {

            item.iswarehouse = $scope.SearchCondition.iswarehouse;
            var dialog = ngDialog.open({
                template: 'ViewDetails',
                data: {
                    'Item': item,
                    'scope': $scope
                },
                width: '700px',
                controller: ['$scope', '$timeout', function ($scope, $timeout) {

                    $scope.Transfer = function () {
                        var list = [];
                        $.each($scope.Rows, function (k, v) {
                            if (v.selected)
                                list.push(v);
                        });
                        $scope.closeThisDialog({ 'action': 'Transfer', 'Rows': list, 'Item': $scope.ngDialogData.Item });
                    }

                    $scope.ChangeSHD = function () {
                        var list = [];
                        $.each($scope.Rows, function (k, v) {
                            if (v.selected)
                                list.push(v);
                        });
                        $scope.closeThisDialog({ 'action': 'ChangeSHD', 'Rows': list, 'Item': $scope.ngDialogData.Item });
                    }

                    $scope.DebitNote = function () {
                        var list = [];
                        $.each($scope.Rows, function (k, v) {
                            if (v.selected)
                                list.push(v);
                        });
                        $scope.closeThisDialog({ 'action': 'DebitNote', 'Rows': list, 'Item': $scope.ngDialogData.Item });
                    }

                    $scope.SelectAll = function (selected) {
                        $.each($scope.Rows, function (k, v) {
                            v.selected = selected;
                            if (v.selected && typeof (v.sqty) == 'undefined')
                                v.sqty = v.qty;
                        });
                    }

                    $scope.Rows = [];
                    var request = {
                        "item": $scope.ngDialogData.Item,
                        "securitytoken": "String content"
                    }

                    $scope.AllowTransfer = !$scope.ngDialogData.Item.iswarehouse;

                    var srequest = JSON.stringify(request);

                    $http({
                        method: 'POST',
                        url: "WarehouseReportService/ROLStoreViewDetails",
                        data: srequest
                    }).success(function (result) {
                        $scope.Rows = result;
                    }).error(function (result) {
                    });

                    $scope.Selected = function () {
                        $scope.closeThisDialog();
                    };
                }],
                className: 'ngdialog-theme-plain'
            });

            dialog.closePromise.then(function (data) {
                {
                    if (typeof (data.value) != 'undefined' && data.value != null && data.value != '$escape' && data.value != '$document') {

                        if (data.value.action == 'Transfer') {
                            $scope.Transfer(data.value.Item, data.value.Rows);
                        }
                        else if (data.value.action == 'DebitNote') {
                            $state.go('PurchaseReturn', {
                                directlaunch: false,
                                data: {
                                    'locationid': $scope.SearchCondition.locationid,
                                    'companyid': $scope.SearchCondition.companyid,
                                    'list': data.value.Rows
                                }
                            });
                        }
                        else if (data.value.action == 'ChangeSHD') {
                            
                        }
                    }
                }
            });
        }

        $scope.Transfer = function (item, items) {
            var dialog = ngDialog.open({
                template: 'ChooseLocation',
                data: {
                    'scope': $scope
                },
                width: '500px',
                controller: ['$scope', '$timeout', function ($scope, $timeout) {
                    $scope.RetialLocations = $scope.ngDialogData.scope.Locations;
                    $scope.BuyingCompany = $scope.ngDialogData.scope.BuyingCompany;
                    $scope.Selected = function () {
                        $scope.closeThisDialog({
                            'fromlocationid': $scope.fromlocationid,
                            'fromcompanyid': $scope.fromcompanyid,
                            'tolocationid': $scope.tolocationid,
                            'tocompanyid': $scope.tocompanyid
                        });
                    };
                }],
                className: 'ngdialog-theme-plain'
            });
            dialog.closePromise.then(function (data) {
                {
                    if (typeof (data.value) != 'undefined' && data.value != null
                        && data.value != '$escape' && data.value != '$document') {
                        var tolocation = $scope.Locations.find(function (item) {
                            return item.id == data.value.tolocationid;
                        });
                        var bundle = {
                            "id": 0,
                            "items_json": null,
                            "Items": [],
                            "selectAll": false,
                            "code": null,
                            "companyid": 0,
                            "fromlocationid": 0,
                            "tolocationid": 0,
                            "packedby": 0,
                            "receivedby": 0,
                            "packedon": null,
                            "notes": null,
                            "approvedby": 0,
                            "isactive": true,
                            "createdon": "",
                            "modifiedon": "",
                            "createdby": 0,
                            "modifiedby": 0,
                            "status": 0,
                            "version": 1,
                            "cansave": true,
                            "candelete": false,
                            "salablegoods": [],
                            "checkList": true,
                            tocompanyid: 0
                        };

                        bundle.companyid = data.value.fromcompanyid;
                        bundle.fromlocationid = data.value.fromlocationid;
                        bundle.tocompanyid = data.value.tocompanyid;
                        bundle.tolocationid = data.value.tolocationid;
                        bundle.inward2warehouse = tolocation.additionalinfo && tolocation.additionalinfo != null && tolocation.additionalinfo.iswarehouse;
                        $.each(items, function (k, v) {
                            bundle.Items.push({
                                'id': v.id, 'aqty': v.sqty, 'barcode': v.barcode, 'supplierid': v.supplierid,
                                'stockid': v.stockid, 'companyid': v.companyid, 'itemid': v.itemid,
                                'lrreference': v.lrreference, 'locationid': v.locationid
                            });
                        });

                        var request = {
                            "Item": bundle,
                            "securitytoken": "String content"
                        }

                        var srequest = JSON.stringify(request);

                        $http({
                            method: 'POST',
                            url: "BundlesService/Outward",
                            data: srequest
                        }).success(function (result) {

                            if (result != null && result.id > 0) {
                                $rootScope.Toaster.Info("[ "+result.code+" ] OutWards Saved Successfully.!");
                            }
                            else {
                                $rootScope.Toaster.Error(result.errormessage);
                            }

                        }).error(function (result) {
                        });
                    }
                }
            });
        }


        $scope.ManageSearchItem = function (item) {
            $scope.DetailedResult = [];
            request = {
                "item": item,
                "securitytoken": "String content"
            }

            var srequest = JSON.stringify(request);

            $http({
                method: 'POST',
                url: "WarehouseReportService/ROLStoreViewDetails",
                data: srequest
            }).success(function (result) {
                if (result != 'undefined' && result.length > 0) {
                    $scope.DetailedResult = result;
                    $scope.printDiv('productviewprintdiv', 'VIEWDETAILS');
                }
            }).error(function (result) {
            });

        }

        $scope.printDiv = function (div, title) {

            $timeout(function () {

                var otitle = angular.element('#apptitle').context.title;
                var titlele = angular.element('#apptitle').context.title = title;

                var docHead = document.head.outerHTML;
                var printContents = document.getElementById(div).outerHTML;
                var winAttr = "location=yes, statusbar=no, menubar=no, titlebar=no, toolbar=no,dependent=no, width=865, height=600, resizable=yes, screenX=200, screenY=200, personalbar=no, scrollbars=yes";

                var newWin = window.open("", "_blank", winAttr);
                var writeDoc = newWin.document;
                writeDoc.open();
                writeDoc.write('<!doctype html><html>' + docHead + '<body onLoad="window.print()">' + printContents + '</body></html>');
                writeDoc.close();
                newWin.focus();

                angular.element('#apptitle').context.title = otitle;
            }, 100);

        }


    }]);