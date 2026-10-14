var app = angular.module('InfinityLab');

app.controller('TailoringInwardController', ['UserDetails', '$scope', '$http', '$location', '$rootScope', '$window', '$filter', 'restService', '$timeout', 'Excel',
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
            receivedfrom: 0,
            fromdate: "",
            todate: "",
            entryno: "",
            orderno: ""
        }
        $scope.SearchBundles = function () {
            request = {
                "Item": $scope.SearchCondition,
                "securitytoken": "String content"
            }

            var srequest = JSON.stringify(request);
            $http({
                method: 'POST',
                url: "TailoringitemsreceivedService/SearchBundle",
                data: srequest
            }).success(function (result) {

                if ($scope.bundlesummary && $scope.bundlesummary != null)
                    $scope.bundlesummary.destroy();

                var bundlesummary = new InfyGrid('outwardContainer', '450px', '100%', true, false, true, 220);
                bundlesummary.options.showHeaderRow = true;

                bundlesummary.AddSimpleColumn('inwardno', 'Inward Bundle No', 120);
                bundlesummary.AddFormatedColumn('receivedon', 'Date', 100, 'dd-MM-yyyy');
                bundlesummary.AddFormatedColumn('receivedqty', 'Qty', 70, 'N2', 2).total = true;

                var commands = "";
                commands += "<input type='button' class='btn btn-primary btn-xs' value='Show'/> &nbsp;"
                bundlesummary.AddButtonColumn('action', 'ACTION', 70, commands, function (e, args) {
                    $scope.$apply(function () {
                        $scope.ShowBundleDetails(args.item.inwardno);
                    });
                });
                bundlesummary.Initialize();
                $scope.bundlesummary = bundlesummary;
                $scope.bundlesummary.UpdateData(result);

            }).error(function (result) {
            });
        }

        $scope.ShowBundleDetails = function (inwardno) {

            request = {
                "Item": { 'inwardno': inwardno },
                "securitytoken": "String content"
            }

            var srequest = JSON.stringify(request);
            $http({
                method: 'POST',
                url: "TailoringitemsreceivedService/Search",
                data: srequest
            }).success(function (result) {
                if ($scope.bundleitems != null)
                    $scope.bundleitems.destroy();
                var bundleitems = new InfyGrid('InwardItemsContainer', '450px', '100%', true, false, true, 5);
                bundleitems.options.showHeaderRow = true;

                bundleitems.AddSimpleColumn('sno', 'S.No', 50);
                bundleitems.AddSimpleColumn('orderno', 'Order No', 70);
                bundleitems.AddSimpleColumn('entryno', 'Entry No', 70);
                bundleitems.AddSimpleColumn('customername', 'Customer', 250);
                bundleitems.AddSimpleColumn('product', 'Product', 250);
                bundleitems.AddSimpleColumn('charge', 'Charge', 80, 'N2', 2).total = true;
                bundleitems.AddFormatedColumn('expectedqty', 'Received', 80, 'N2', 2).total = true;
                bundleitems.AddFormatedColumn('pendingqty', 'Pending', 80, 'N2', 2).total = true;
                bundleitems.AddFormatedColumn('receivedqty', 'Inward', 80, 'N2', 2).total = true;

                bundleitems.Initialize();
                $scope.bundleitems = bundleitems;
                $scope.bundleitems.UpdateData(result);
            }).error(function (result) {
            });

        }

        $scope.ComputeTotal = function () {
            $scope.TailoringInward.pendingqty = 0;
            $scope.TailoringInward.receivedqty = 0;
            $scope.TailoringInward.charges = 0;
            $.each($scope.InwardItems, function (k, v) {
                $scope.TailoringInward.pendingqty += v.pendingqty;
                $scope.TailoringInward.receivedqty += v.receivedqty;
                $scope.TailoringInward.charges += v.charge;
            })
        }

        $scope.DeleteItem = function (item) {
            $scope.InwardItems.splice($scope.InwardItems.indexOf(item), 1);
            $scope.OutwordSummary = [];
            $.each($scope.InwardItems, function (k, v) {
                var summary = $scope.OutwordSummary.find(function (item) {
                    return item.product == v.product;
                });

                if (typeof (summary) == 'undefined' || summary == null) {
                    summary = { 'product': v.product, 'qty': v.receivedqty, 'items': [] };
                    summary.items.push(v);
                    $scope.OutwordSummary.push(summary);
                }
                else {
                    summary.items.push(v);
                    summary.qty += v.receivedqty;
                }
            });

            $scope.igrid.UpdateData($scope.InwardItems);
            $scope.isummary.UpdateData($scope.OutwordSummary);
            $scope.ComputeTotal();
        }

        $scope.GridConfig = function () {
            if ($scope.isummary != null)
                $scope.isummary.destroy();

            var isummary = new InfyGrid('summaryContainer', '450px', '100%', true, false, true, 280);
            isummary.options.showHeaderRow = true;

            isummary.AddSimpleColumn('product', 'Product', 200);
            isummary.AddFormatedColumn('qty', 'Qty', 70, 'N2', 2).total = true;

            var commands = "";
            commands += "<input type='button' class='btn btn-primary btn-xs' value='Delete'/> &nbsp;"
            isummary.AddButtonColumn('action', 'ACTION', 70, commands, function (e, args) {
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
            igrid.AddSimpleColumn('customername', 'Customer', 250);
            igrid.AddSimpleColumn('product', 'Product', 250);
            igrid.AddSimpleColumn('charge', 'Charge', 80, 'N2', 2).total = true;
            igrid.AddFormatedColumn('expectedqty', 'Received', 80, 'N2', 2).total = true;
            igrid.AddFormatedColumn('pendingqty', 'Pending', 80, 'N2', 2).total = true;
            igrid.AddFormatedColumn('receivedqty', 'Inward', 80, 'N2', 2).total = true;

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

        $scope.InwardItems = [];
        $scope.OutwordSummary = [];

        $scope.Clear = function () {
            $scope.InwardItems = [];
            $scope.OutwordSummary = [];
            $scope.igrid.UpdateData($scope.InwardItems);
            $scope.isummary.UpdateData($scope.OutwordSummary);
        }

        $scope.Save = function () {

            if ($scope.TailoringInward.receivedby == 'undefined' || $scope.TailoringInward.receivedby == null 
                || $scope.TailoringInward.receivedby == 0)
            {
                $rootScope.Toaster.Error("Please enter received user name.");
                return;
            }
            if ($scope.TailoringInward.receivedsupplierid == 'undefined' || $scope.TailoringInward.receivedsupplierid == null
                || $scope.TailoringInward.receivedsupplierid == 0) {
                $rootScope.Toaster.Error("Please enter tailoring unit location.");
                return;
            }
            if ($scope.TailoringInward.locationid == 'undefined' || $scope.TailoringInward.locationid == null
                || $scope.TailoringInward.locationid == 0) {
                $rootScope.Toaster.Error("Please enter receiving store location.");
                return;
            }

            $.each($scope.InwardItems, function (k, v) {
                v.receivedby = $scope.TailoringInward.receivedby;
                v.receivedfrom = $scope.TailoringInward.receivedsupplierid;
                v.receivedon = $scope.TailoringInward.receivedon;
            })

            request = {
                "Items": $scope.InwardItems,
                "securitytoken": "String content"
            }

            var srequest = JSON.stringify(request);
            $http({
                method: 'POST',
                url: "TailoringitemsreceivedService/SaveBundle",
                data: srequest
            }).success(function (result) {
                if (result != null && result.length > 0) {
                    $rootScope.Toaster.Info("Order saved Successfully ");
                    $scope.InwardItems = result;
                    $scope.igrid.UpdateData($scope.InwardItems);
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
                    'locationid': $scope.TailoringInward.locationid,
                    'companyid': $scope.TailoringInward.companyid,
                    'entryno': entryno, 'orderno': orderno,
                    'fromdate': entryno != '' && orderno != '' ? $scope.TailoringInward.fromdate : null,
                    'todate': entryno != '' && orderno != '' ? $scope.TailoringInward.todate : null,
                    'showdetail': true,
                    'forinward': true
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

                    var existorder = $scope.InwardItems.find(function (item) {
                        return item.tailoringorderid == v.id;
                    });

                    if (typeof (existorder) == 'undefined' || existorder == null) {
                        $.each(v.Tailoringitems, function (kk, vv) {

                            if (vv.issuedqty > vv.receivedqty) {
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
                                    'pendingqty': vv.issuedqty - vv.receivedqty,
                                    'receivedqty': vv.issuedqty - vv.receivedqty,
                                    'sno': $scope.InwardItems.length + 1,
                                    'locationid': v.locationid,
                                    'isactive': true,
                                    'version': 1
                                };

                                $scope.InwardItems.push(outwarditem);

                                var summary = $scope.OutwordSummary.find(function (item) {
                                    return item.product == outwarditem.product;
                                });

                                if (typeof (summary) == 'undefined' || summary == null) {
                                    summary = { 'product': outwarditem.product, 'qty': outwarditem.receivedqty, 'items': [] };
                                    summary.items.push(outwarditem);
                                    $scope.OutwordSummary.push(summary);
                                }
                                else {
                                    summary.items.push(outwarditem);
                                    summary.qty += outwarditem.receivedqty;
                                }
                            }
                        })
                    }
                });

                $scope.igrid.UpdateData($scope.InwardItems);
                $scope.isummary.UpdateData($scope.OutwordSummary);
                $scope.ComputeTotal();

                if ($scope.InwardItems.length == 0) {
                    $rootScope.Toaster.Info("No Records found for Inward!");
                }
            });
        }

        $timeout(function () {
            $scope.GridConfig();
        }, 10);
    }]);

