
var app = angular.module('InfinityLab');

app.controller('TailoringOutwardController', ['UserDetails', '$scope', '$http', '$location', '$rootScope', '$window', '$filter', 'restService', '$timeout', 'Excel',
    function (UserDetails, $scope, $http, $location, $rootScope, $window, $filter, restService, $timeout, Excel) {
        $scope.Navigation = $rootScope.GetNavigation();
        $scope.ShowSearch = false;
        $scope.SearchRef = restService.SearchReference;

        $scope.ErrorMessage = "";

        $scope.Employees = restService.Get('ReferencelistService/EMPLOY', true).then(function (results) {
            $scope.Employees = results;
        });
        $scope.Location = restService.Get('ReferencelistService/RETAILLOCATION', true).then(function (results) {
            $scope.Location = results;
        });
        $scope.Company = restService.Get('ReferencelistService/BUYINGCOMPANY', true).then(function (results) {
            $scope.Company = results;
        });
        $scope.TailoringUnit = restService.Get('ReferencelistService/TAILORINGUNIT', true).then(function (results) {
            $scope.TailoringUnit = results;
        });
        $scope.SearchCondition = {
            companyid: 0,
            locationid: 0,
            stitchinglocationid: 0,
            fromdate: "",
            todate: ""
        }

        $scope.SearchBundles = function () {
            request = {
                "Item": $scope.SearchCondition,
                "securitytoken": "String content"
            }

            var srequest = JSON.stringify(request);
            $http({
                method: 'POST',
                url: "TailoringitemsissuedService/SearchBundle",
                data: srequest
            }).success(function (result) {

                if ($scope.bundlesummary && $scope.bundlesummary != null)
                    $scope.bundlesummary.destroy();

                var bundlesummary = new InfyGrid('outwardContainer', '450px', '100%', true, false, true, 190);
                bundlesummary.options.showHeaderRow = true;

                bundlesummary.AddSimpleColumn('outwardno', 'Bundle No', 80);
                bundlesummary.AddFormatedColumn('issuedon', 'Date', 80, 'dd-MM-yyyy');
                bundlesummary.AddFormatedColumn('issuedqty', 'Qty', 70, 'N2', 2).total = true;

                var commands = "";
                commands += "<input type='button' class='btn btn-primary btn-xs' value='Show'/> &nbsp;"
                bundlesummary.AddButtonColumn('action', 'ACTION', 70, commands, function (e, args) {
                    $scope.$apply(function () {
                        $scope.ShowBundleDetails(args.item.outwardno);
                    });
                });
                bundlesummary.Initialize();
                $scope.bundlesummary = bundlesummary;
                $scope.bundlesummary.UpdateData(result);

            }).error(function (result) {
            });
        }

        $scope.ShowBundleDetails = function (outwardno) {

            request = {
                "Item": $scope.SearchCondition,
                "securitytoken": "String content"
            }

            var srequest = JSON.stringify(request);
            $http({
                method: 'POST',
                url: "TailoringitemsissuedService/Search",
                data: srequest
            }).success(function (result) {
                if ($scope.bundleitems != null)
                    $scope.bundleitems.destroy();
                var bundleitems = new InfyGrid('outwarditemsContainer', '450px', '100%', true, false, true, 20);
                bundleitems.options.showHeaderRow = true;

                bundleitems.AddSimpleColumn('sno', 'S.No', 50);
                bundleitems.AddSimpleColumn('orderno', 'Order Ref No', 100);
                bundleitems.AddSimpleColumn('entryno', 'Entry No', 100);
                bundleitems.AddSimpleColumn('outwardno', 'Bundle No', 100);
                bundleitems.AddFormatedColumn('issuedon', 'Bundle Date', 100, 'dd/MM/yyyy');
                bundleitems.AddReferenceColumn('issuedby', 'Issued By', 100, $scope.Employees);
                bundleitems.AddSimpleColumn('product', 'Product', 250);
                //bundleitems.AddSimpleColumn('charge', 'Charge', 80, 'N2', 2).total = true;
                //bundleitems.AddFormatedColumn('sourceqty', 'Received', 80, 'N2', 2).total = true;
                //bundleitems.AddFormatedColumn('expectedqty', 'Qty', 80, 'N2', 2).total = true;
                bundleitems.AddFormatedColumn('issuedqty', 'Transfered Qty', 80, 'N2', 2).total = true;

                bundleitems.Initialize();
                $scope.bundleitems = bundleitems;
                $scope.bundleitems.UpdateData(result);
            }).error(function (result) {
            });

        }

        $scope.ComputeTotal = function () {
            $scope.TailoringOutward.issuedqty = 0;
            //$scope.TailoringOutward.receivedqty = 0;
            //$scope.TailoringOutward.deliveredqty = 0;
            //$scope.TailoringOutward.charges = 0;
            $.each($scope.OutwardItems, function (k, v) {
                $scope.TailoringOutward.issuedqty += $rootScope.GetNumber(v.outwardqty);
                //$scope.TailoringOutward.receivedqty += v.sourceqty;
                //$scope.TailoringOutward.deliveredqty += v.issuedqty;
                //$scope.TailoringOutward.charges += v.charge;
            })
        }

        $scope.GridConfig = function () {
            if ($scope.isummary != null)
                $scope.isummary.destroy();

            var isummary = new InfyGrid('summaryContainer', '450px', '100%', true, false, true, 210);
            isummary.options.showHeaderRow = true;

            isummary.AddSimpleColumn('product', 'Product', 220);
            isummary.AddFormatedColumn('qty', 'Qty', 70, 'N2', 2).total = true;

            var commands = "";
            commands += "<input type='button' class='btn btn-primary btn-xs' value='Delete'/> &nbsp;"
            isummary.AddButtonColumn('action', 'ACTION', 90, commands, function (e, args) {
                $scope.$apply(function () {
                    $scope.DeleteSummary(args.item);
                });
            });
            isummary.Initialize();
            $scope.isummary = isummary;
            $scope.isummary.UpdateData([]);


            if ($scope.igrid != null)
                $scope.igrid.destroy();

            var igrid = new InfyGrid('gridContainer', '450px', '100%', true, false, true, 50);
            igrid.options.showHeaderRow = true;

            igrid.AddSimpleColumn('sno', 'S.No', 50);
            igrid.AddSimpleColumn('orderno', 'Order No', 70);
            igrid.AddSimpleColumn('entryno', 'Entry No', 70);
            igrid.AddSimpleColumn('customername', 'Customer', 120);
            igrid.AddSimpleColumn('product', 'Product', 120);
            igrid.AddFormatedColumn('charge', 'Charge', 80, 'N2', 2).total = true;
            //igrid.AddFormatedColumn('sourceqty', 'Received', 80, 'N2', 2).total = true;
            igrid.AddFormatedColumn('expectedqty', 'Qty', 80, 'N2', 2).total = true;
            igrid.AddFormatedColumn('issuedqty', 'Issued', 80, 'N2', 2).total = true;
            //igrid.AddFormatedColumn('outwardqty', 'Transfer Qty', 80, 'N2', 2).total = true;

            igrid.AddColumn(
                {
                    id: "Transfer", name: "Transfer Qty", field: "outwardqty",
                    width: 80, cssClass: "cell-title", editor: Slick.Editors.Text,
                    'sortable': true
                }
            ).total = true;

            var commands = "";
            commands += "<input type='button' class='btn btn-primary btn-xs' value='Delete'/> &nbsp;"
            igrid.AddButtonColumn('action', 'ACTION', 80, commands, function (e, args) {
                $scope.$apply(function () {
                    $scope.DeleteItem(args.item);
                });
            });
            igrid.Initialize();
            $scope.igrid = igrid;
            $scope.igrid.UpdateData([]);
        }

        $scope.OutwardItems = [];
        $scope.OutwordSummary = [];

        $scope.DeleteItem = function (item) {
            $scope.OutwardItems.splice($scope.OutwardItems.indexOf(item), 1);
            $scope.TailoringOutward.issuedqty = 0;
            $scope.OutwordSummary = [];
            $.each($scope.OutwardItems, function (k, v) {
                $scope.TailoringOutward.issuedqty += $rootScope.GetNumber(v.outwardqty);

                var summary = $scope.OutwordSummary.find(function (item) {
                    return item.product == v.product;
                });

                if (typeof (summary) == 'undefined' || summary == null) {
                    summary = { 'product': v.product, 'qty': v.outwardqty, 'items': [] };
                    summary.items.push(v);
                    $scope.OutwordSummary.push(summary);
                }
                else {
                    summary.items.push(v);
                    summary.qty += v.issuedqty;
                }
            });           

            $scope.igrid.UpdateData($scope.OutwardItems);
            $scope.isummary.UpdateData($scope.OutwordSummary);
            $scope.ComputeTotal();
        }

        $scope.Clear = function () {
            $scope.OutwardItems = [];
            $scope.OutwordSummary = [];
            $scope.igrid.UpdateData($scope.OutwardItems);
            $scope.isummary.UpdateData($scope.OutwordSummary);
        }

        $scope.Save = function () {

            $.each($scope.OutwardItems, function (k, v) {
                v.issuedby = $scope.TailoringOutward.issuedby;
                v.issuedto = $scope.TailoringOutward.issuedto;
                v.issuedon = $scope.TailoringOutward.issuedon;
            })

            request = {
                "Items": $scope.OutwardItems,
                "securitytoken": "String content"
            }

            var srequest = JSON.stringify(request);
            $http({
                method: 'POST',
                url: "TailoringitemsissuedService/SaveBundle",
                data: srequest
            }).success(function (result) {
                if (result != null && result.length > 0) {
                    $rootScope.Toaster.Info("Order saved Successfully with bundle No : " + result[0].outwardno);
                    $scope.OutwardItems = result;
                    $scope.igrid.UpdateData($scope.OutwardItems);
                }
                else {
                    $rootScope.Toaster.Error('Unexpected error');
                }
            }).error(function (result) {
            });
        }

        $scope.AddOrder = function (entryno, orderno) {
            var request = {
                "Item": {
                    'locationid': $scope.TailoringOutward.locationid,
                    'companyid': $scope.TailoringOutward.companyid,
                    'stitchinglocationid': $scope.TailoringOutward.stitchinglocationid,
                    'entryno': $scope.TailoringOutward.entryno,
                    'orderno': $scope.TailoringOutward.orderno,
                    'fromdate': $scope.TailoringOutward.fromdate,
                    'todate': $scope.TailoringOutward.todate,
                    'showdetail': true,
                    'foroutward': true
                },
                "securitytoken": "String content"
            }

            var srequest = JSON.stringify(request);
            $http({
                method: 'POST',
                url: "TailoringOrderService/Search",
                data: srequest
            }).success(function (result) {
                $.each(result, function (k, v) {

                    var existorder = $scope.OutwardItems.find(function (item) {
                        return item.tailoringorderid == v.id;
                    });

                    if (typeof (existorder) == 'undefined' || existorder == null) {
                        $.each(v.Tailoringitems, function (kk, vv) {

                            if (vv.expectedqty > vv.issuedqty) {
                                var outwarditem = {
                                    'tailoringorderid': v.id,
                                    'tailoringitemsid': vv.id,
                                    'customername': v.customername,
                                    'product': vv.product,
                                    'orderno': v.orderno,
                                    'entryno': v.entryno,
                                    'charge': vv.total,
                                    'sourcemeter': vv.sourcemeter,
                                    'sourceqty': vv.sourceqty,
                                    'expectedqty': vv.expectedqty,
                                    'issuedqty': vv.issuedqty,
                                    'receivedqty': vv.receivedqty,
                                    'outwardqty': vv.expectedqty - vv.issuedqty,
                                    'sno': $scope.OutwardItems.length + 1,
                                    'locationid': v.locationid,
                                    'isactive': true,
                                    'version': 1
                                };

                                $scope.OutwardItems.push(outwarditem);

                                var summary = $scope.OutwordSummary.find(function (item) {
                                    return item.product == outwarditem.product;
                                });

                                if (typeof (summary) == 'undefined' || summary == null) {
                                    summary = { 'product': outwarditem.product, 'qty': outwarditem.outwardqty, 'items': [] };
                                    summary.items.push(outwarditem);
                                    $scope.OutwordSummary.push(summary);
                                }
                                else {
                                    summary.items.push(outwarditem);
                                    summary.qty += outwarditem.outwardqty;
                                }
                            }
                        })
                    }
                });

                $scope.TailoringOutward.issuedqty = 0;
                $.each($scope.OutwardItems, function (k, v) {
                    $scope.TailoringOutward.issuedqty += $rootScope.GetNumber(v.outwardqty);
                });
                
                $scope.igrid.UpdateData($scope.OutwardItems);
                $scope.isummary.UpdateData($scope.OutwordSummary);
                $scope.ComputeTotal();

                if ($scope.OutwardItems.length == 0) {
                    $rootScope.Toaster.Info("Zero Items found!");
                }
            });
        }

        $timeout(function () {
            $scope.GridConfig();
        }, 10);
    }]);

