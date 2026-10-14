var app = angular.module('InfinityLab');


app.controller('TailoringOrderController', ['UserDetails', '$scope', '$http', '$location', '$rootScope', '$window', 'restService', 'Excel', '$timeout', '$filter', 'ngDialog', 'localStorageService',
    function (UserDetails, $scope, $http, $location, $rootScope, $window, restService, Excel, $timeout, $filter, ngDialog, localStorageService) {

        $scope.Navigation = $rootScope.GetNavigation();
        $scope.Navigation.IsDelivery = false;
        $scope.ShowCharge = false;
        $scope.ShowSearch = false;
        $scope.ShowPendingBills = false;
        $scope.SearchResult = [];
        $scope.ViewMode = 'INBOX';
        $rootScope.AccordionName = $location.path();
        $scope.SearchRef = restService.SearchReference;
        $scope.DefaultMeasuresments = [];

        $scope.StatusList = [{ 'id': '1', 'name': 'Order Received' },
        { 'id': '2', 'name': 'Outward for Stitching' },
        { 'id': '3', 'name': 'Ready for Delivery' },
        { 'id': '4', 'name': 'Delivered' },
        { 'id': '5', 'name': 'Partially Delivered' },
        { 'id': '6', 'name': 'Unpaid' },
        { 'id': '7', 'name': 'Paid' },
        { 'id': '8', 'name': 'Payment' },
        ];

        $scope.PaymentModes = [{ 'id': '1', 'name': 'Cash' },
        { 'id': '2', 'name': 'Card' },
        { 'id': '3', 'name': 'Online' }];

        $scope.Products = restService.Get('ReferencelistService/PRODUCTS', true).then(function (results) {
            $scope.Products = results;
        });
        $scope.Employees = restService.Get('ReferencelistService/EMPLOY', true).then(function (results) {
            $scope.Employees = results;
        });
        $scope.Tailors = restService.Get('ReferencelistService/TAILORS', true).then(function (results) {
            $scope.Tailors = results;
        });
        $scope.LocationList = restService.Get('ReferencelistService/RETAILLOCATION', true).then(function (results) {
            $scope.LocationList = results;
        });
        $scope.TailoringUnit = restService.Get('ReferencelistService/TAILORINGUNIT', true).then(function (results) {
            $scope.TailoringUnit = results;
        });
        $scope.BuyingCompany = restService.Get('ReferencelistService/BUYINGCOMPANY', true).then(function (results) {
            $scope.BuyingCompany = results;
        });
        $scope.Suppliers = restService.Get('ReferencelistService/SUPPLIER', true).then(function (results) {
            $scope.Suppliers = results;
        });
        $scope.TaxList = restService.Get('ReferencelistService/SALESTAX', true).then(function (results) {
            $scope.TaxList = results;
        });

        $scope.SwitchPanel = function () {
            $scope.ShowSearch = !$scope.ShowSearch;
            $scope.TailoringOrder = (JSON.parse(JSON.stringify($scope.Entity)));
            $scope.Navigation.IsDelivery = false;
            $scope.DeliveryProducts = [];
        }

        $scope.ProductChargeList = restService.Get('ReferencelistService/FULL-TAILORINGCHARGE', true).then(function (results) {
            $scope.ProductChargeList = results;
            $.each(results, function (k, v) {
                v.measurement = [];
                v.charge = v.additionalinfo.mfgconfig.charge;
                v.taxid = v.additionalinfo.mfgconfig.taxid;
                v.additionalchargeid = v.additionalinfo.mfgconfig.additionalchargeid;
                v.additionalchargeamount = v.additionalinfo.mfgconfig.additionalchargeamount;
                if (v.additionalinfo.mfgconfig &&
                    v.additionalinfo.mfgconfig.measurement &&
                    v.additionalinfo.mfgconfig.measurement != null) {
                    $.each(v.additionalinfo.mfgconfig.measurement.split(','), function (kk, vv) {
                        v.measurement.push({
                            'key': vv, 'value': '', 'focusonenter': 'mfe_' + k, 'focusid': 'mf_' + k, 'islast': false
                        });
                    });
                }
                if (v.measurement.length > 0)
                    v.measurement[v.measurement.length - 1].islast = true;
            });
        });

        $scope.QtyChanged = function () {
            var qty = $scope.GetNumber($scope.SelectedTailoringitems.expectedqty);
            $scope.SelectedTailoringitems.gross = $scope.SelectedTailoringitems.charge * qty;
            $scope.SelectedTailoringitems.tax = $scope.SelectedTailoringitems.taxpercentage > 0 ? ($scope.SelectedTailoringitems.gross * $scope.SelectedTailoringitems.taxpercentage / 100) : 0;
            $scope.SelectedTailoringitems.total = $scope.SelectedTailoringitems.gross + $scope.SelectedTailoringitems.tax;
            $scope.ShowCharge = ($scope.SelectedTailoringitems.total && !isNaN($scope.SelectedTailoringitems.total) && $scope.SelectedTailoringitems.total > 0);
        }

        $scope.CalculateTotal = function () {
            $scope.TailoringOrder.expectedqty = 0;
            $scope.TailoringOrder.gross = 0;
            $scope.TailoringOrder.tax = 0;
            $scope.TailoringOrder.total = 0;
            $scope.TailoringOrder.paymenttotal = 0;
            $scope.TailoringOrder.additionalcharge = 0;

            $.each($scope.TailoringOrder.Tailoringitems, function (k, v) {
                $scope.TailoringOrder.additionalcharge += ($scope.GetNumber(v.expectedqty) * $scope.GetNumber(v.additionalchargeamount));
                $scope.TailoringOrder.expectedqty += $scope.GetNumber(v.expectedqty);
                $scope.TailoringOrder.expectedqty += $scope.GetNumber(v.expectedqty);
                $scope.TailoringOrder.gross += v.gross;
                $scope.TailoringOrder.tax += v.tax;
                $scope.TailoringOrder.total += v.total;
            });

            if ($scope.TailoringOrder.TailoringPayments && $scope.TailoringOrder.TailoringPayments != null &&
                $scope.TailoringOrder.TailoringPayments.length > 0) {
                $.each($scope.TailoringOrder.TailoringPayments, function (k, v) {
                    $scope.TailoringOrder.paymenttotal += $scope.GetNumber(v.amount);
                });
            }

            $scope.TailoringOrder.paymentbalance = $scope.TailoringOrder.total - $scope.TailoringOrder.paymenttotal;
        }

        $scope.UpdateMeasureConfig = function () {
            var product = $scope.ProductChargeList.find(function (item) {
                return item.id == $scope.SelectedTailoringitems.measurementid;
            });

            if (product && product != null) {
                $scope.SelectedTailoringitems.product = product.name;
                $scope.SelectedTailoringitems.measurementname = product.name;
                $scope.SelectedTailoringitems.measurement = (JSON.parse(JSON.stringify(product.measurement)));
                $scope.SelectedTailoringitems.charge = product.charge;
                $scope.SelectedTailoringitems.taxid = product.taxid;
                $scope.SelectedTailoringitems.chargeid = product.id;
                $scope.SelectedTailoringitems.taxpercentage = 0;
                $scope.SelectedTailoringitems.additionalchargeid = product.additionalchargeid;
                $scope.SelectedTailoringitems.additionalchargeamount = product.additionalchargeamount;

                var existing = $scope.DefaultMeasuresments.find(function (item) {
                    return item.chargeid == product.id;
                });

                if (typeof (existing) != 'undefined' && existing != null) {
                    $.each($scope.SelectedTailoringitems.measurement, function (k, v) {
                        var keyval = existing.measurement.find(function (item) {
                            return item.key == v.key;
                        });
                        if (typeof (keyval) != 'undefined' && keyval != null) {
                            v.value = keyval.value;
                        }
                    });
                }

                var tax = $scope.TaxList.find(function (item) {
                    return item.id == product.taxid;
                });
                if (tax && tax != null) {
                    $scope.SelectedTailoringitems.taxpercentage = tax.taxpercentage;
                }
                $scope.QtyChanged();
                $timeout(function () {
                    if ($scope.setFocusmesmnt_0)
                        $scope.setFocusmesmnt_0();
                }, 30);
            }
        }

        $scope.DeliveryProducts = [];

        $scope.Entity = {
            "id": 0,
            "companyid": 0,
            "orderno": null,
            "receivedon": new Date(),
            "collectedon": new Date(),
            "status": '1',
            "supplierid": 0,
            "status": 0,
            "customername": "",
            "customermobile": "",
            "expectedon": new Date(),
            "isactive": true,
            "createdby": 0,
            "modifiedby": 0,
            "version": 1,
            "createdon": "0001-01-01T00:00:00",
            "modifiedon": "0001-01-01T00:00:00",
            "Tailoringitems": []
        };
        $scope.SelectedTailoringitems = {
            "tailoringorderid": 0, "sourceproductid": 0, "tailorid": 0,
            "expectedqty": 0, "expectedqty": 0, "expectedqty": 0,
            "charge": 0.0, "measurement": 0,
            "modifiedon": "", "createdon": "",
            "createdby": 0, "modifiedby": 0, "version": 1, "isactive": true,
            "measurement": []
        };
        $scope.TailoringOrder = (JSON.parse(JSON.stringify($scope.Entity)));

        $scope.UpdateTailor = function () {
            var request = {
                "Items": [],
                "securitytoken": "String content"
            }

            if ($scope.TailoringOrder.Tailoringitems &&
                $scope.TailoringOrder.Tailoringitems != null) {
                $.each($scope.TailoringOrder.Tailoringitems, function (k, v) {
                    if (v.tailorid && v.tailorid > 0) {
                        request.Items.push({ 'id': v.id, 'tailorid': v.tailorid });
                    }
                    else {
                        request.Items.push({ 'id': v.id, 'tailorid': 0 });
                    }
                })
            }

            var srequest = JSON.stringify(request);
            $http({
                method: 'POST',
                url: "TailoringitemsService/UpdateTailor",
                data: srequest
            }).success(function (result) {
                $rootScope.Toaster.Info("Tailor changed Successfully ");
            }).error(function (result) {
            });
        }

        $scope.AddPayment = function () {
            if (typeof ($scope.TailoringOrder.TailoringPayments) == 'undefined' || $scope.TailoringOrder.TailoringPayments == null)
                $scope.TailoringOrder.TailoringPayments = [];

            var paymentitem = {
                'collectedon': $scope.TailoringOrder.collectedon,
                'paymode': $scope.TailoringOrder.paymode,
                'amount': $scope.TailoringOrder.amount,
                'isactive': true,
                'version': 1,
                'tailoringid': $scope.TailoringOrder.id
            };
            $scope.TailoringOrder.TailoringPayments.push(paymentitem);
            $scope.TailoringOrder.amount = 0;
            $scope.CalculateTotal();
        }

        $scope.changedexpectedon = function () {
            var date = new Date($scope.TailoringOrder.receivedon);
            date = date.setDate(date.getDate() + 7);
            $scope.TailoringOrder.expectedon = $filter('date')(date, datePicker.shortDisplay);
        }

        $scope.AddItems = function () {

            //if ($scope.SelectedTailoringitems.barcodes == undefined ||
            //    $scope.SelectedTailoringitems.barcodes == null ||
            //    $scope.SelectedTailoringitems.barcodes.length == 0) {
            //    $rootScope.Toaster.Notice('Please add the barcode of tailoring items');
            //        return;
            //    }


            var tailor = $scope.Tailors.find(function (item) {
                return item.id == $scope.SelectedTailoringitems.tailorid;
            });

            if (tailor && tailor != null &&
                tailor.config && tailor.config != null) {

                var costconfig = tailor.config.find(function (item) {
                    return item.chargeid == $scope.SelectedTailoringitems.measurementid;
                });

                if (costconfig && costconfig != null) {
                    $scope.SelectedTailoringitems.charge = costconfig.charge;
                    $scope.SelectedTailoringitems.delayperday = costconfig.delayperday;
                    $scope.SelectedTailoringitems.delaymaximum = costconfig.delaymaximum;
                    $scope.SelectedTailoringitems.chargeonpriority = costconfig.chargeonpriority;
                }
            }



            if (typeof ($scope.TailoringOrder.Tailoringitems) == 'undefined' || $scope.TailoringOrder.Tailoringitems == null)
                $scope.TailoringOrder.Tailoringitems = [];
            var list = [];
            $.each($scope.SelectedTailoringitems.measurement, function (k, v) {
                list.push(v.value);
            });

            var qty = $scope.GetNumber($scope.SelectedTailoringitems.expectedqty);
            $scope.SelectedTailoringitems.gross = $scope.SelectedTailoringitems.charge * qty;
            $scope.SelectedTailoringitems.tax = $scope.SelectedTailoringitems.taxpercentage > 0 ? ($scope.SelectedTailoringitems.gross * $scope.SelectedTailoringitems.taxpercentage / 100) : 0;
            $scope.SelectedTailoringitems.total = $scope.SelectedTailoringitems.gross + $scope.SelectedTailoringitems.tax;
            $scope.SelectedTailoringitems.measurementdetails = list.join('-');
            $scope.SelectedTailoringitems.isactive = true;
            $scope.TailoringOrder.Tailoringitems.push($scope.SelectedTailoringitems);
            $scope.SelectedTailoringitems = {
                "tailoringorderid": 0, "sourceproductid": 0, "tailorid": 0,
                "expectedqty": 0, "expectedqty": 0, "expectedqty": 0,
                "charge": 0.0, "measurement": 0,
                "modifiedon": "", "createdon": "",
                "createdby": 0, "modifiedby": 0, "version": 1, "isactive": true,
                "measurement": [], "barcodes": []
            }
            $scope.ShowItemGrid();
            $scope.ShowBarcodeGrid();
            $scope.CalculateTotal();
        }

        $scope.SearchCondition = {
            "fromdate": new Date(),
            "todate": new Date()
        }
        $scope.SearchCustomer = function () {

            var letter = $scope.sale_customercardprefix;
            if ($scope.system_supportdeliveryrack &&
                ($scope.TailoringOrder.customermobile.length > 2 &&
                    $scope.TailoringOrder.customermobile.startsWith('#') &&
                    $scope.TailoringOrder.customermobile.endsWith('#'))) {
                $scope.ResolveRack($scope.TailoringOrder.customermobile.substring(1, $scope.TailoringOrder.customermobile.length - 1), '');
                return;
            }

            if ($scope.system_supportdeliveryrack &&
                ($scope.TailoringOrder.customermobile.length > 2 &&
                    $scope.TailoringOrder.customermobile.startsWith($scope.sale_deliverycardprefix))) {
                $scope.ResolveRack('', $scope.TailoringOrder.customermobile);
                return;
            }


            $scope.TailoringOrder.customerid = 0;
            var request = {
                "Item": {
                    'mobileno': $scope.TailoringOrder.customermobile
                },
                "SecurityToken": "String content"
            }
            var srequest = JSON.stringify(request);
            $http({
                method: 'POST',
                url: "CustomerService/Search",
                data: srequest
            }).success(function (result) {
                if (result.length == 1) {

                    $scope.DefaultMeasuresments = [];
                    var customer = result[0];
                    $scope.TailoringOrder.cityid = customer.cityid;
                    $scope.TailoringOrder.customeraddress = customer.address;
                    $scope.TailoringOrder.customername = customer.name;
                    $scope.TailoringOrder.customerid = customer.id;
                }
            });
        }


        $scope.qEncodeURIComponent = function (value) {
            if (value == undefined || value == '')
                value = '-';
            if (value.indexOf('.') >= 0)
                value = value.replaceAll('.', '~dot~');
            if (value.indexOf('/') >= 0)
                value = value.replaceAll('/', '~sls~');
            return encodeURIComponent(value);
        }


        $scope.ResolveRack = function (rackcode, cardno, callback) {

            restService.Get('CustomerService/SearchByRackCard/' + $scope.qEncodeURIComponent(rackcode) +
                '/' + $scope.qEncodeURIComponent(cardno) +
                '/' + $rootScope.LocationInfo.location.id, true).then(function (results) {
                    if (results != 'undefined' && results != null) {

                        $scope.TailoringOrder.rackid = results.rackid;
                        $scope.TailoringOrder.customerid = results.customerid;
                        $scope.TailoringOrder.cardno = results.cardno;
                        $scope.TailoringOrder.rackno = results.rackno;
                        $scope.TailoringOrder.customermobile = results.mobileno;
                        $scope.TailoringOrder.customeraddress = results.address;
                        $scope.TailoringOrder.customername = results.name;

                    }
                    else {
                        $rootScope.Toaster.Notice('Cant find the valid token');
                    }

                });
        }


        $scope.New = function () {
            $scope.ViewMode = 'CREATE';
            $scope.TailoringOrder = (JSON.parse(JSON.stringify($scope.Entity)));
            $scope.TailoringOrder.IsDelivery = false;
            $scope.SelectedTailoringitems = {
                measurement: {},
                barcodes: []
            };
            $scope.DeliveryProducts = [];
            $timeout(function () {
                $scope.ShowItemGrid();
                $scope.ShowBarcodeGrid();
            }, 30)
        }

        $scope.SearchOrders = function () {

            if ($scope.SearchCondition.status == 6 || $scope.SearchCondition.status == 7) {
                $scope.SearchPayment();
                return;
            }

            $scope.OrderList = [];
            $scope.DefaultMeasuresments = [];
            request = {
                "Item": $scope.SearchCondition,
                "securitytoken": "String content"
            }

            var srequest = JSON.stringify(request);
            $http({
                method: 'POST',
                url: "TailoringOrderService/Search",
                data: srequest
            }).success(function (result) {
                $scope.OrderList = result;
                $scope.OrderConfig();
            }).error(function (result) {
            });
        }

        $scope.paymentcount = 0;
        $scope.SearchPayment = function () {
            request = {
                "Item": $scope.SearchCondition,
                "securitytoken": "String content"
            }

            var srequest = JSON.stringify(request);
            $http({
                method: 'POST',
                url: "TailoringOrderService/SearchPayment",
                data: srequest
            }).success(function (result) {
                $scope.paymentcount = 0;
                if ($scope.igrid != null)
                    $scope.igrid.destroy();

                var igrid = new InfyGrid('gridContainer', '450px', '100%', true, true, true, 20);
                igrid.options.showHeaderRow = true;
                igrid.EnableSerial(result);

                igrid.AddSimpleColumn('entryno', 'Order No', 80);
                igrid.AddSimpleColumn('tailorname', 'Tailor Name', 140);
                igrid.AddFormatedColumn('orderdate', 'Order Date', 100, 'dd-MM-yyyy');
                igrid.AddFormatedColumn('expectedon', 'Due Date', 100, 'dd-MM-yyyy');
                igrid.AddFormatedColumn('deliveredon', 'Delivered Date', 100, 'dd-MM-yyyy');
                igrid.AddSimpleColumn('product', 'Product', 140);
                igrid.AddSimpleColumn('measurementname', 'Item', 140);
                igrid.AddSimpleColumn('designname', 'Design', 140);
                igrid.AddFormatedColumn('charge', 'Charge', 60, 'N2', 2);
                igrid.AddFormatedColumn('deliveredqty', 'Qty', 60, 'N2', 2);
                igrid.AddFormatedColumn('amount', 'Amount', 60, 'N2', 2).total = true;
                igrid.AddFormatedColumn('paid', 'Paid', 80, 'N2', 2).total = true;
                igrid.AddFormatedColumn('paidon', 'Paid Date', 100, 'dd-MM-yyyy');
                igrid.AddFormatedColumn('balance', 'Balance', 70, 'N2', 2).total = true;
                igrid.EnableTotal();
                igrid.Initialize();

                igrid.onSelectedRangesChanged = function (range, rowmodel) {
                    $timeout(function () {
                        $scope.paymentcount = range.length;
                        $scope.ComputePayment();
                    }, 10);
                };

                $scope.igrid = igrid;
                $scope.igrid.UpdateData(result, '_id');

            }).error(function (result) {
            });
        }

        $scope.inboxcount = 0;
        $scope.ShowPayment = false;
        $scope.SearchInbox = function () {
            request = {
                "Item": $scope.SearchCondition,
                "securitytoken": "String content"
            }

            var srequest = JSON.stringify(request);
            $http({
                method: 'POST',
                url: "TailoringOrderService/SearchInbox",
                data: srequest
            }).success(function (result) {
                $scope.inboxcount = 0;
                if ($scope.inboxgrid != null)
                    $scope.inboxgrid.destroy();
                $scope.ShowPaymentPrint = false;
                $scope.ShowPayment = ($scope.SearchCondition.status == 6 || $scope.SearchCondition.status == 7);
                if ($scope.SearchCondition.status == 8)
                    $scope.ShowPaymentGrid(result);
                else
                    $scope.ShowOrderGrid(result);

            }).error(function (result) {
            });
        }

        $scope.ShowPaymentGrid = function (result) {
            $scope.ShowPaymentPrint = true;
            var igrid = new InfyGrid('gridInboxContainer', '450px', '100%', true, true, true, 20);
            igrid.options.showHeaderRow = true;
            igrid.EnableSerial(result);

            igrid.AddSimpleColumn('entryno', 'Entry No', 80);
            igrid.AddSimpleColumn('orderno', 'Order No', 80);
            igrid.AddSimpleColumn('tailorname', 'Tailor Name', 200);
            igrid.AddFormatedColumn('orderdate', 'Order Date', 100, 'dd-MM-yyyy');
            igrid.AddFormatedColumn('collectedon', 'Paid Date', 100, 'dd-MM-yyyy');
            igrid.AddSimpleColumn('paymentcode', 'Receipt No', 80);
            
            igrid.AddSimpleColumn('customername', 'Customer', 150);
            igrid.AddSimpleColumn('measurementname', 'Item', 200);
            igrid.AddFormatedColumn('receivedqty', 'Qty', 70, 'N2', 2);
            igrid.AddFormatedColumn('charge', 'Charge', 70, 'N2', 2);
            var exist = result.find(function (item) { return item.prioritycharge > 0; });
            if (exist && exist != null) {
                igrid.AddFormatedColumn('prioritycharge', 'Priority', 60, 'N2', 2);
            }
            var exist = result.find(function (item) { return item.delaycharge > 0; });
            if (exist && exist != null) {
                igrid.AddFormatedColumn('delaycharge', 'Priority', 60, 'N2', 2);
            }
            igrid.AddFormatedColumn('amount', 'Amount', 80, 'N2', 2).total = true;

            igrid.EnableTotal();
            igrid.Initialize();

            igrid.setCellCSS('igridbigrows');

            igrid.onSelectedRangesChanged = function (range, rowmodel) {
                $timeout(function () {
                    $scope.inboxcount = range.length;
                }, 10);
            };

            $scope.inboxgrid = igrid;
            $scope.inboxgrid.UpdateData(result, '_id');
        }

        $scope.ShowOrderGrid = function (result) {
            var igrid = new InfyGrid('gridInboxContainer', '450px', '100%', true, true, true, 20);
            igrid.options.showHeaderRow = true;
            igrid.EnableSerial(result);

            igrid.AddSimpleColumn('entryno', 'Entry No', 80);
            igrid.AddSimpleColumn('orderno', 'Order No', 80);
            igrid.AddSimpleColumn('tailorname', 'Tailor Name', 140);
            igrid.AddFormatedColumn('orderdate', 'Order Date', 100, 'dd-MM-yyyy');
            igrid.AddFormatedColumn('orderdate', 'Order Date', 100, 'dd-MM-yyyy');
            igrid.AddFormatedColumn('expectedon', 'Due Date', 100, 'dd-MM-yyyy');
            igrid.AddFormatedColumn('deliveredon', 'Delivered Date', 100, 'dd-MM-yyyy');

            igrid.AddSimpleColumn('billno', 'Bill No', 100);
            if (!$scope.ShowPayment) {
                igrid.AddSimpleColumn('customername', 'Customer', 150);
                igrid.AddSimpleColumn('customermobile', 'Contact', 100);
            }
            igrid.AddSimpleColumn('product', 'Product', 100);
            igrid.AddSimpleColumn('measurementname', 'Item', 100);
            igrid.AddSimpleColumn('designname', 'Design', 100);
            igrid.AddSimpleColumn('notes', 'Notes', 100);
            igrid.AddFormatedColumn('expectedqty', 'Qty', 60, 'N2', 2);
            igrid.AddFormatedColumn('receivedqty', 'Ready', 60, 'N2', 2);
            igrid.AddFormatedColumn('deliveredqty', 'Delivered', 60, 'N2', 2);
            if ($scope.ShowPayment) {
                igrid.AddFormatedColumn('charge', 'Charge', 60, 'N2', 2);
                var exist = result.find(function (item) { return item.prioritycharge > 0; });
                if (exist && exist != null) {
                    igrid.AddFormatedColumn('prioritycharge', 'Priority', 60, 'N2', 2);
                }
                var exist = result.find(function (item) { return item.delaycharge > 0; });
                if (exist && exist != null) {
                    igrid.AddFormatedColumn('delaycharge', 'Priority', 60, 'N2', 2);
                }
                igrid.AddFormatedColumn('amount', 'Amount', 60, 'N2', 2).total = true;
                igrid.AddFormatedColumn('paid', 'Paid', 80, 'N2', 2).total = true;
                igrid.AddFormatedColumn('paidon', 'Paid Date', 100, 'dd-MM-yyyy');
                igrid.AddFormatedColumn('balance', 'Balance', 70, 'N2', 2).total = true;
            }
            igrid.EnableTotal();
            igrid.Initialize();

            igrid.setCellCSS('igridbigrows');
            igrid.dataView.getItemMetadata = function (row) {
                var rowdata = igrid.grid.getData(
                ).getItem(row);

                if (rowdata.receivedqty == 0) {
                    rowdata.style = 'igridbigrowswithmark';
                    return { 'cssClasses': '' };
                }

                if (rowdata.receivedqty > 0 &&
                    rowdata.receivedqty == rowdata.deliveredqty) {
                    rowdata.style = 'igridbigrowswithred';
                    return { 'cssClasses': '' };
                }

                rowdata.style = 'igridbigrowswithgreen';
                return { 'cssClasses': '' };
            };

            igrid.onSelectedRangesChanged = function (range, rowmodel) {
                $timeout(function () {
                    $scope.inboxcount = range.length;
                    var seletedRows = $scope.inboxgrid.GetSelectedRows();
                    var rqty = 0;
                    var dqty = 0;
                    var pamt = 0;
                    $.each(seletedRows, function (k, v) {
                        rqty += (v.expectedqty - v.receivedqty);
                        dqty += (v.receivedqty - v.deliveredqty);
                        pamt += v.balance;
                    })
                    $scope.inboxrqty = rqty;
                    $scope.inboxdqty = dqty;
                    $scope.inboxpamt = pamt;
                }, 10);
            };

            igrid.grid.onDblClick.subscribe(function (e, args) {
                var item = args.grid.getDataItem(args.row);
                var field = args.grid.getColumns()[args.cell].id;
                if (field == 'measurementname') {
                    $timeout(function () {
                        $scope.ShowMeasurement(item);
                    }, 10);
                }
                if (field == 'expectedqty') {
                    $timeout(function () {
                        if (item.expectedqty - item.receivedqty > 0)
                            $scope.SplitQty(item);
                    }, 10);
                }
                if (field == 'receivedqty') {
                    $timeout(function () {
                        if (item.tailorid && item.tailorid > 0 &&
                            item.expectedqty - item.receivedqty > 0)
                            $scope.ReceivedQty(item);
                    }, 10);
                }
                if (field == 'deliveredqty') {
                    $timeout(function () {
                        if (item.tailorid && item.tailorid > 0 &&
                            (item.receivedqty - item.deliveredqty) > 0)
                            $scope.Deliveredqty(item);
                    }, 10);
                }
            });
            $scope.inboxgrid = igrid;
            $scope.inboxgrid.UpdateData(result, '_id');
        }

        $scope.PrintPayment = function () {
            var paymentcode = null;
            $.each($scope.inboxgrid.GetSelectedRows(), function (k, v) {
                if (paymentcode == null)
                    paymentcode = v.paymentcode;
            });
            $rootScope.PostDownload('TailoringOrderService/PrintPayment', {
                'Item': { 'paymentcode': paymentcode }},
                'TailorPayment.pdf', 'application/pdf');
        }


        $scope.DeletePayment = function () {
            var list = [];
            $.each($scope.inboxgrid.GetSelectedRows(), function (k, v) {
                list.push({ 'id': v.id });
            });
            var request = {
                "Items": list,
                "securitytoken": "String content"
            }
            var srequest = JSON.stringify(request);
            $http({
                method: 'POST',
                url: "TailoringOrderService/CancelPayment",
                data: srequest
            }).success(function (result) {
                $rootScope.Toaster.Info("Cancelled");
                $scope.SearchInbox();
            }).error(function (result) {
            });
        }

        $scope.ShowMeasurement = function (item) {


            var dialog = ngDialog.open({
                template: 'ShowMeasurement',
                data: {
                    'scope': $scope,
                    'item': item
                },
                width: '500px',
                controller: ['$scope', '$timeout', function ($scope, $timeout) {
                    $scope.Tailoringitem = $scope.ngDialogData.item;
                    $scope.MeasurementList = $scope.ngDialogData.item.measurement;
                }],
                className: 'ngdialog-theme-plain'
            });
            dialog.closePromise.then(function (data) {
                {

                }
            });
        }

        $scope.Deliveredqty = function (item) {

            var ids = [];
            if (item == undefined || item == null) {
                $.each($scope.inboxgrid.GetSelectedRows(), function (k, v) {
                    ids.push(v.id);
                });
            }

            var dialog = ngDialog.open({
                template: 'Deliveredqty',
                data: {
                    'scope': $scope,
                    'showqty': item && item != null,
                    'ids': ids,
                    'id': (item && item != null) ? item.id : 0,
                },
                width: '500px',
                controller: ['$scope', '$timeout', function ($scope, $timeout) {
                    $scope.Employees = $scope.ngDialogData.scope.Employees;
                    $scope.ids = $scope.ngDialogData.ids;
                    $scope.id = $scope.ngDialogData.id;
                    $scope.showqty = $scope.ngDialogData.showqty;
                    $scope.Delivery = { 'allids': $scope.ids, 'id': $scope.id };
                    $scope.Change = function () {
                        $scope.closeThisDialog($scope.Delivery);
                    }
                }],
                className: 'ngdialog-theme-plain'
            });
            dialog.closePromise.then(function (data) {
                {

                    if (typeof (data.value) == 'undefined' || data.value == null || data.value == '$document' || data.value == '$escape')
                        return;

                    var request = {
                        "Item": data.value,
                        "securitytoken": "String content"
                    }

                    var srequest = JSON.stringify(request);
                    $http({
                        method: 'POST',
                        url: "TailoringitemsService/DeliverQty",
                        data: srequest
                    }).success(function (result) {
                        $rootScope.Toaster.Info("Delivery Successfully Changed");
                        $scope.SearchInbox();
                    }).error(function (result) {
                    });

                }
            });
        }

        $scope.AssignTailor = function () {
            var seletedRows = $scope.inboxgrid.GetSelectedRows();
            var ids = [];
            $.each(seletedRows, function (k, v) {
                ids.push({ 'id': v.id, 'tailorid': 0 });
            })
            var dialog = ngDialog.open({
                template: 'AssignTailor',
                data: {
                    'scope': $scope,
                    'ids': ids
                },
                width: '300px',
                controller: ['$scope', '$timeout', function ($scope, $timeout) {
                    $scope.Tailors = $scope.ngDialogData.scope.Tailors;
                    $scope.ids = $scope.ngDialogData.ids;
                    $scope.Change = function () {
                        $.each($scope.ids, function (k, v) {
                            v.tailorid = $scope.tailorid;
                        });
                        $scope.closeThisDialog($scope.ids);
                    }
                }],
                className: 'ngdialog-theme-plain'
            });
            dialog.closePromise.then(function (data) {
                {

                    if (typeof (data.value) == 'undefined' || data.value == null || data.value == '$document' || data.value == '$escape')
                        return;

                    var request = {
                        "Items": data.value,
                        "securitytoken": "String content"
                    }

                    var srequest = JSON.stringify(request);
                    $http({
                        method: 'POST',
                        url: "TailoringitemsService/UpdateTailor",
                        data: srequest
                    }).success(function (result) {
                        $rootScope.Toaster.Info("Tailor changed Successfully ");
                        $scope.SearchInbox();
                    }).error(function (result) {
                    });

                }
            });
        }

        $scope.ReceiveAll = function () {
            var seletedRows = $scope.inboxgrid.GetSelectedRows();
            var request = {
                "Item": { 'allids': [] },
                "securitytoken": "String content"
            }

            var missingsno = [];
            $.each(seletedRows, function (k, v) {
                if (v.tailorid && v.tailorid > 0)
                    request.Item.allids.push(v.id);
                else
                    missingsno.push(v.serial);
            })

            if (missingsno.length > 0) {
                $rootScope.Toaster.Error("Unassigned tailor items can't be complegted - " + missingsno.join(','));
                return;
            }

            var srequest = JSON.stringify(request);
            $http({
                method: 'POST',
                url: "TailoringitemsService/ReceivedQty",
                data: srequest
            }).success(function (result) {
                $rootScope.Toaster.Info("Received Successfully ");
                $scope.SearchInbox();
            }).error(function (result) {
            });
        }

        $scope.ReceivedQty = function (item) {
            $rootScope.Prompt('Receive', 'Qty', function (count) {
                var totalcount = $rootScope.GetNumber(count);
                if (totalcount > 0) {
                    if ((item.expectedqty - item.receivedqty) >= totalcount) {

                        item.receivedqty = totalcount;
                        var request = {
                            "Item": item,
                            "securitytoken": "String content"
                        }

                        var srequest = JSON.stringify(request);
                        $http({
                            method: 'POST',
                            url: "TailoringitemsService/ReceivedQty",
                            data: srequest
                        }).success(function (result) {
                            $rootScope.Toaster.Info("Received Successfully ");
                            $scope.SearchInbox();
                        }).error(function (result) {
                        });

                    }
                    else {
                        $rootScope.Toaster.Error("Can't Received More qty, allowed only available qty " + (item.expectedqty - item.receivedqty));
                    }
                }
            });
        }

        $scope.SplitQty = function (item) {
            $rootScope.Prompt('Split', 'Qty', function (count) {
                var totalcount = $rootScope.GetNumber(count);
                if (totalcount > 0) {

                    if ((item.expectedqty - item.receivedqty) >= totalcount) {

                        item.splitqty = totalcount;
                        var request = {
                            "Item": item,
                            "securitytoken": "String content"
                        }

                        var srequest = JSON.stringify(request);
                        $http({
                            method: 'POST',
                            url: "TailoringitemsService/SplitQty",
                            data: srequest
                        }).success(function (result) {
                            $rootScope.Toaster.Info("Spltted Successfully ");
                            $scope.SearchInbox();
                        }).error(function (result) {
                        });

                    }
                    else {
                        $rootScope.Toaster.Error("Can't split, allowed only available qty " + (item.expectedqty - item.receivedqty));
                    }


                }
            }, item.expectedqty);
        }

        $scope.ProcessPayment = function () {
            var seletedRows = $scope.inboxgrid.GetSelectedRows();
            var rows = [];
            $.each(seletedRows, function (k, v) {

                if (v.tailorname == null || v.tailorname == '')
                    return;

                var exist = rows.find(function (item) {
                    return item.tailorname == v.tailorname;
                });

                if (exist == undefined || exist == null) {
                    exist = {
                        'tailorname': v.tailorname,
                        'tailorid': v.tailorid,
                        'amount': 0, 'paid': 0, 'balance': 0, 'TailoringItems': []
                    };
                    rows.push(exist);
                }

                exist.amount += v.amount;
                exist.paid += v.paid;
                exist.TailoringItems.push({ 'id': v.id, 'amount': v.balance });
                exist.balance += v.balance;
            });

            var dialog = ngDialog.open({
                template: 'Payment',
                data: {
                    'scope': $scope,
                    'rows': rows
                },
                width: '700px',
                controller: ['$scope', '$timeout', '$rootScope', 'restService',
                    function ($scope, $timeout, $rootScope, restService) {
                        $scope.rows = $scope.ngDialogData.rows;
                        $scope.pamt = 0;
                        $scope.PaymentModes = [{ 'id': 0, 'name': 'Cash' }];
                        $scope.BuyingCompany = $scope.ngDialogData.scope.BuyingCompany;
                        $scope.LocationList = $scope.ngDialogData.scope.LocationList;
                        restService.Get('ReferencelistService/BANK', true).then(function (results) {
                            results.unshift({ 'id': 0, 'name': 'Cash' });
                            $scope.PaymentModes = results;
                        });

                        $scope.Save = function () {
                            var payment = {
                                'paymode': $scope.paymode,
                                'amount': $scope.pamt,
                                'date': $scope.pdate,
                                'locationid': $scope.locationid,
                                'companyid': $scope.companyid,
                                'items': []
                            };

                            var tpaidamount = 0;
                            var extraamount = false;
                            $.each($scope.rows, function (k, v) {
                                var paidamount = parseFloat(v.newpaid);
                                if (isNaN(paidamount)) paidamount = 0;
                                if (paidamount > 0) {
                                    tpaidamount += paidamount;
                                    payment.items.push(v);
                                }
                                $.each(v.TailoringItems, function (kk, vv) {
                                    vv.newpaid = 0;
                                })
                                if (paidamount > 0) {
                                    $.each(v.TailoringItems, function (kk, vv) {
                                        if (vv.amount <= paidamount)
                                            vv.newpaid = vv.amount
                                        else
                                            vv.newpaid = paidamount;
                                        paidamount -= vv.newpaid;
                                    })
                                }
                                if (paidamount > 0) {
                                    extraamount = true;
                                }
                            })

                            if (extraamount) {
                                $rootScope.Toaster.Info("Can't accept more than balance amount");
                                return;
                            }


                            if (payment.items.length > 0 && tpaidamount > 0 && tpaidamount == $scope.pamt)
                                $scope.closeThisDialog(payment);
                        }

                        $scope.ShowGrid = function () {

                            var igrid = new InfyGrid('dlgpaymentContainer', '200px', '100%', true, true, true);
                            igrid.options.showHeaderRow = true;
                            igrid.EnableSerial(rows);
                            igrid.AddSimpleColumn('tailorname', 'Tailor Name', 140);
                            igrid.AddFormatedColumn('amount', 'Amount', 60, 'N2', 2).total = true;
                            igrid.AddFormatedColumn('paid', 'Paid', 80, 'N2', 2).total = true;
                            igrid.AddFormatedColumn('balance', 'Balance', 70, 'N2', 2).total = true;
                            var dcolumn = igrid.AddCustomColumn('fullvalue','', 50, function (item, col) {
                                    if (item.fullvalue)
                                        return '<input type="checkbox" style="z-index: auto;margin-left:15px" checked>';
                                    else
                                        return '<input type="checkbox" style="z-index: auto;margin-left:15px">';
                                });
                            igrid.AddColumn(
                                {
                                    id: "newpaid", name: "New", field: "newpaid",
                                    width: 60, cssClass: "cell-title", editor: Slick.Editors.Text,
                                    'sortable': true
                                }
                            );

                            igrid.EnableTotal();
                            igrid.options.autoEdit = true;

                            igrid.Initialize();
                            igrid.grid.onCellChange.subscribe(function (e, args) {
                                $timeout(function () {
                                    var tpaidamount = 0;
                                    $.each($scope.rows, function (k, v) {
                                        var paidamount = parseFloat(v.newpaid);
                                        if (isNaN(paidamount)) paidamount = 0;
                                        tpaidamount += paidamount;
                                    })
                                    $scope.pamt = tpaidamount;
                                }, 10);
                            });

                            igrid.grid.onClick.subscribe(function (e, args) {
                                if (args.grid.getColumns()[args.cell].id == 'fullvalue') {
                                    item = args.grid.getDataItem(args.row);
                                    item.fullvalue = !item.fullvalue && $(e.target).val() == 'on';
                                    item.newpaid = item.fullvalue ? (item.balance) : 0;
                                   
                                    $timeout(function () {

                                            var tpaidamount = 0;
                                            $.each($scope.rows, function (k, v) {
                                                var paidamount = parseFloat(v.newpaid);
                                                if (isNaN(paidamount)) paidamount = 0;
                                                tpaidamount += paidamount;
                                            })
                                            $scope.pamt = tpaidamount;
                                  

                                        args.grid.invalidate();
                                        args.grid.render();
                                    }, 10);
                                }
                            });

                            $scope.pgrid = igrid;
                            $scope.pgrid.UpdateData($scope.rows, '_id');
                        }
                        $timeout(function () {
                            $scope.ShowGrid();
                            $('#dlgpaymentContainer .slick-pane-top').css('top', '25px');
                        }, 10);
                    }],
                className: 'ngdialog-theme-plain'
            });
            dialog.closePromise.then(function (data) {
                {
                    if (typeof (data.value) == 'undefined' || data.value == null || data.value == '$document' || data.value == '$escape')
                        return;

                    var request = {
                        "Item": data.value,
                        "securitytoken": "String content"
                    }

                    var srequest = JSON.stringify(request);
                    $http({
                        method: 'POST',
                        url: "TailoringitemsService/TailoringPayment",
                        data: srequest
                    }).success(function (result) {
                        $rootScope.Toaster.Info("Delivery Successfully Changed");
                        $scope.SearchInbox();
                    }).error(function (result) {
                    });
                }
            });

        }

        $scope.ComputePayment = function () {
            var seletedRows = this.igrid.GetSelectedRows();
            var rows = [];
            $.each(seletedRows, function (k, v) {

                if (v.tailorname == null || v.tailorname == '')
                    return;

                var exist = rows.find(function (item) {
                    return item.tailorname == v.tailorname;
                });

                if (exist == undefined || exist == null) {
                    exist = { tailorname: v.tailorname, 'amount': 0, 'paid': 0, 'balance': 0, 'ids': [] };
                    rows.push(exist);
                }

                exist.amount += v.amount;
                exist.paid += v.paid;
                exist.ids.push(v.id);
                exist.balance += v.balance;
            });

            if ($scope.pgrid != null)
                $scope.pgrid.destroy();

            var igrid = new InfyGrid('paymentContainer', '450px', '100%', true, true, true, 290);
            igrid.options.showHeaderRow = true;
            igrid.EnableSerial(rows);

            igrid.AddSimpleColumn('tailorname', 'Tailor Name', 140);
            igrid.AddFormatedColumn('amount', 'Amount', 60, 'N2', 2).total = true;
            igrid.AddFormatedColumn('paid', 'Paid', 80, 'N2', 2).total = true;
            igrid.AddFormatedColumn('balance', 'Balance', 70, 'N2', 2).total = true;
            igrid.EnableTotal();
            igrid.Initialize();
            $scope.pgrid = igrid;
            $scope.pgrid.UpdateData(rows, '_id');
        }

        $scope.AddBarcode = function () {
            if ($scope.SelectedTailoringitems.barcodes == undefined || $scope.SelectedTailoringitems.barcodes == null)
                $scope.SelectedTailoringitems.barcodes = [];

            $scope.SelectedTailoringitems.barcodes.push({
                'barcode': $scope.SelectedTailoringitems.barcode,
                'meter': $scope.SelectedTailoringitems.meter,
                'pcs': $scope.SelectedTailoringitems.pcs,
            });
            $scope.ShowBarcodeGrid();
            $scope.SelectedTailoringitems.barcode = '';
            $scope.SelectedTailoringitems.meter = '';
            $scope.SelectedTailoringitems.pcs = '';
            $scope.setFocusbarcode();
        }


        $scope.ShowItemGrid = function () {
            if ($scope.igrid != null)
                $scope.igrid.destroy();

            var igrid = new InfyGrid('itemgrid', '450px', '100%', true, true, true, 190);
            igrid.options.showHeaderRow = true;
            igrid.AddSimpleColumn('product', 'Product', 140);
            igrid.AddFormatedColumn('sourceqty', 'Meter', 60, 'N2', 2);
            igrid.AddSimpleColumn('designname', 'Design', 140);

            igrid.AddFormatedColumn('expectedqty', 'Delivery Qty', 100, 'N2', 2);
            if ($scope.system_tailoringunits) {
                igrid.AddFormatedColumn('issuedqty', 'Issued Qty', 100, 'N2', 2);
            }

            igrid.AddFormatedColumn('receivedqty', 'Ready Qty', 100, 'N2', 2);
            igrid.AddFormatedColumn('deliveredqty', 'Delivered Qty', 100, 'N2', 2);

            igrid.AddFormatedColumn('charge', 'Charge', 100, 'N2', 2);

            igrid.AddColumn(
                {
                    id: "priorityqty", name: "Priority", field: "priorityqty",
                    width: 60, cssClass: "cell-title", editor: Slick.Editors.Text,
                    'sortable': true
                }
            )

            igrid.AddSimpleColumn('measurementdetails', 'Measurement', 200);
            igrid.AddButtonColumn('Delete', '', 35, "<button type='button' style='width:30px;height:25px;margin-right:5px;line-height: 0;' value='Delete' class='btn btn-danger btn-xs'><i class='fa fa-trash' style='font:normal normal normal 14px/1 FontAwesome'></i></button>");
            igrid.AddButtonColumn('Edit', '', 35, "<button type='button' style='width:30px;height:25px;margin-right:5px;line-height: 0;' value='Edit' class='btn btn-warning btn-xs'><i class='fa fa-edit' style='font:normal normal normal 14px/1 FontAwesome'></i></button>");

            igrid.EnableTotal();
            igrid.Initialize();
            $scope.igrid = igrid;

            igrid.grid.onCellChange.subscribe(function (e, args) {

                var item = args.grid.getDataItem(args.row);
                if (args.grid.getColumns()[args.cell].id == 'charge') {
                    var qty = $scope.GetNumber(item.expectedqty);
                    item.gross = item.charge * qty;
                    item.tax = item.taxpercentage > 0 ? (item.gross * item.taxpercentage / 100) : 0;
                    item.total = item.gross + item.tax;
                    $timeout(function () {
                        args.grid.invalidateRow(args.row);
                        args.grid.render();
                    }, 10);
                }

            });
            igrid.grid.onClick.subscribe(function (e) {
                var cell = igrid.grid.getCellFromEvent(e);
                var item = igrid.grid.getDataItem(cell.row);
                var command = igrid.grid.getColumns()[cell.cell].id;
                if (command == 'Delete' || command == 'Edit') {
                    $scope.$apply(function () {
                        $scope.ManageItem(command, item);
                    });
                }
                return true;
            });
            $scope.igrid.UpdateData($scope.TailoringOrder.Tailoringitems, '_id');
        }

        $scope.ShowBarcodeGrid = function () {
            if ($scope.bgrid != null)
                $scope.bgrid.destroy();

            var igrid = new InfyGrid('barcodeContainer', '450px', '100%', true, true, true, 420);
            igrid.options.showHeaderRow = true;
            igrid.EnableSerial($scope.SelectedTailoringitems.barcodes);
            igrid.AddSimpleColumn('barcode', 'Barcode', 140);
            igrid.AddFormatedColumn('meter', 'Meter', 60, 'N2', 2);
            igrid.AddFormatedColumn('pcs', 'Paid', 80, 'N2', 2).total = true;
            igrid.EnableTotal();
            igrid.Initialize();
            $scope.bgrid = igrid;
            $scope.bgrid.UpdateData($scope.SelectedTailoringitems.barcodes, '_id');
        }

        $scope.Save = function () {
            if ($scope.TailoringOrder.companyid == null || $scope.TailoringOrder.companyid == 0) {
                $scope['setFocuscompanyid']();
                $rootScope.Toaster.Error("Please enter company name");
                return;
            }
            if ($scope.TailoringOrder.locationid == null || $scope.TailoringOrder.locationid == 0) {
                $scope['setFocuslocationid']();
                $rootScope.Toaster.Error("Please enter location");
                return;
            }
            if ($scope.TailoringOrder.receivedby == null || $scope.TailoringOrder.receivedby == 0) {
                $scope['setFocusreceivedby']();
                $rootScope.Toaster.Error("Please enter received by");
                return;
            }
            if ($scope.TailoringOrder.customermobile == null || $scope.TailoringOrder.customermobile == 0) {
                $scope['setFocusmobileno']();
                $rootScope.Toaster.Error("Please Enter Customer Contact No");
                return;
            }

            if ($scope.TailoringOrder.customername == null || $scope.TailoringOrder.customername == 0) {
                $scope['setFocuscustomername']();
                $rootScope.Toaster.Error("Please enter Customer Name");
                return;
            }

            $scope.CalculateTotal();

            request = {
                "Item": $scope.TailoringOrder,
                "securitytoken": "String content"
            }

            var srequest = JSON.stringify(request);
            $http({
                method: 'POST',
                url: "TailoringorderService/Save",
                data: srequest
            }).success(function (result) {

                if (result != null && result.id > 0) {
                    $rootScope.Toaster.Info("Order saved Successfully ");
                    $scope.TailoringOrder = result;

                    var printerData = localStorageService.get('PrinterData');
                    if (printerData == undefined || printerData == null) {
                        $rootScope.ShowAlert('Printer', 'Please select the printer');
                        return;
                    }

                    if (result.printdocument && result.printdocument != '')
                        $rootScope.Sent2Printer(result.printdocument);
                }
                else {
                    $rootScope.Toaster.Error(result.ErrorMessage);
                }
            }).error(function (result) {
            });

        }

        $scope.Print = function (id) {
            var printerData = localStorageService.get('PrinterData');
            if (printerData == undefined || printerData == null) {
                $rootScope.ShowAlert('Printer', 'Please select the printer');
                return;
            }

            restService.Get('TailoringorderService/Print/' + id, true).then(function (results) {
                if (results && results != '')
                    $rootScope.Sent2Printer(results);
            });
        }

        $scope.OrderList = [];


        $scope.ViewOrder = function (item, callback) {
            $scope.OrderList = [];
            request = {
                "Item": { 'id': item.id },
                "securitytoken": "String content"
            }

            var srequest = JSON.stringify(request);
            $http({
                method: 'POST',
                url: "TailoringOrderService/Search",
                data: srequest
            }).success(function (result) {
                $scope.TailoringOrder = result[0];

                $scope.TailoringOrder.paymenttotal = 0;
                $.each($scope.TailoringOrder.Tailoringitems, function (k, v) {
                    var list = [];
                    $.each(v.measurement, function (k, v) {
                        list.push(v.value);
                    });
                    v.measurementdetails = list.join('-');
                });

                $.each($scope.TailoringOrder.TailoringPayments, function (k, v) {
                    $scope.TailoringOrder.paymenttotal += $scope.GetNumber(v.amount);
                });
                $scope.TailoringOrder.paymentbalance = $scope.TailoringOrder.total - $scope.TailoringOrder.paymenttotal;

                if (typeof (callback) != 'undefined' && callback != null)
                    callback();
                //else if ($scope.TailoringOrder.issuedqty > 0) {
                //    $scope.TailoringOrder.IsDelivery = true;
                //}
                $scope.Navigation.SetMode('EDIT');
                $scope.ShowSearch = false;
                $scope.ViewMode = 'CREATE';

                $timeout(function () {
                    $scope.ShowItemGrid()
                }, 50);

            }).error(function (result) {
            });

        }

        $scope.OrderConfig = function () {
            if ($scope.igrid != null)
                $scope.igrid.destroy();

            var igrid = new InfyGrid('gridContainer', '450px', '100%', true, false, true, 20);
            igrid.options.showHeaderRow = true;
            igrid.EnableSerial($scope.OrderList);

            igrid.AddSimpleColumn('entryno', 'Order No', 80);
            igrid.AddSimpleColumn('orderno', 'Order Ref No', 100);
            igrid.AddSimpleColumn('customermobile', 'Contact No', 120);
            igrid.AddSimpleColumn('customername', 'Customer Name', 140);
            igrid.AddFormatedColumn('receivedon', 'Order Date', 100, 'dd-MM-yyyy');
            igrid.AddFormatedColumn('expectedon', 'Delivery Date', 100, 'dd-MM-yyyy');
            igrid.AddReferenceColumn('status', 'Status', 100, $scope.StatusList);
            igrid.AddFormatedColumn('expectedqty', 'Qty', 60, 'N2', 2).total = true;
            igrid.AddFormatedColumn('issuedqty', 'Outward Qty', 80, 'N2', 2).total = true;
            igrid.AddFormatedColumn('receivedqty', 'Inward Qty', 70, 'N2', 2).total = true;
            igrid.AddFormatedColumn('deliveredqty', 'Delivered Qty', 80, 'N2', 2).total = true;

            var commands = "";
            commands += "<input type='button' class='btn btn-primary btn-xs' value='View'/> &nbsp;"
            commands += "<input type='button' class='btn btn-primary btn-xs' value='Deliver'/> &nbsp;"
            commands += "<input type='button' class='btn btn-primary btn-xs' value='Print'/> &nbsp;"
            igrid.AddButtonColumn('action', 'ACTION', 160, commands, function (e, args) {
                $scope.$apply(function () {
                    if ($(e.target).val() == 'View')
                        $scope.EnableViewOrder(args.item);
                    if ($(e.target).val() == 'Deliver')
                        $scope.EnableDelivery(args.item);
                    if ($(e.target).val() == 'Print')
                        $scope.Print(args.item.id);
                });
            });
            igrid.EnableTotal();
            igrid.Initialize();
            $scope.igrid = igrid;
            $scope.igrid.UpdateData($scope.OrderList, '_id');
        }

        $scope.TailoringOrder.orderdate = new Date();

        $scope.ManageItem = function (action, item) {
            if (action == 'Edit') {
                $scope.SelectedTailoringitems = item;
                $scope.TailoringOrder.Tailoringitems.splice($scope.TailoringOrder.Tailoringitems.indexOf(item), 1);
                if ($scope.SelectedTailoringitems.barcodes == undefined || $scope.SelectedTailoringitems.barcodes == null)
                    $scope.SelectedTailoringitems.barcodes = [];
                $scope.ShowBarcodeGrid($scope.SelectedTailoringitems.barcodes);
            } else if (action == 'Delete') {
                $scope.TailoringOrder.Tailoringitems.splice($scope.TailoringOrder.Tailoringitems.indexOf(item), 1);
            }
            if (action == 'EditPayment') {
                $scope.TailoringOrder.collectedon = item.collectedon;
                $scope.TailoringOrder.paymode = item.paymode;
                $scope.TailoringOrder.amount = item.amount;
                $scope.TailoringOrder.TailoringPayments.splice($scope.TailoringOrder.TailoringPayments.indexOf(item), 1);
            } else if (action == 'DeletePayment') {
                $scope.TailoringOrder.TailoringPayments.splice($scope.TailoringOrder.TailoringPayments.indexOf(item), 1);
            } else if (action == 'DeleteDelivery') {
                if (item.id == 'undefined' || item.id == 0) {
                    $scope.TailoringOrder.TailoringDelivery.splice($scope.TailoringOrder.TailoringDelivery.indexOf(item), 1);
                    $scope.LoadDeliveryProducts();
                }
                else {
                    $rootScope.Toaster.Error("Delivered items can't be altered.");
                    return;
                }
            }
            $scope.CalculateTotal();
        }

        $scope.EnableViewOrder = function (item) {
            $scope.ViewOrder(item, function () {
                //$scope.Navigation.IsDelivery = false;
                $scope.TailoringOrder.IsDelivery = false;
            });
        }

        $scope.EnableDelivery = function (item) {
            $scope.ViewOrder(item, function () {
                //$scope.Navigation.IsDelivery = true;
                $scope.TailoringOrder.IsDelivery = true;
                $scope.TailoringOrder.deliveredon = new Date();
                $scope.LoadDeliveryProducts();
            });
        }

        $scope.LoadDeliveryProducts = function () {
            if (typeof ($scope.TailoringOrder.TailoringDelivery) == 'undefined' ||
                $scope.TailoringOrder.TailoringDelivery == null)
                $scope.TailoringOrder.TailoringDelivery = [];

            $scope.DeliveryProducts = [];
            $.each($scope.TailoringOrder.Tailoringitems, function (k, v) {

                //var plannedqty = 0;
                //$.each($scope.TailoringOrder.TailoringDelivery, function (kk, vv) {
                //    if (vv.id == 0 && vv.tailoringitemsid == v.id) {
                //        plannedqty += vv.deliveredqty;
                //    }
                //});

                //if (v.receivedqty > v.deliveredqty + plannedqty)
                //    $scope.DeliveryProducts.push({
                //        'id': v.id, 'name': v.product + ' : ' + v.designname,
                //        'qty': v.receivedqty - v.deliveredqty,
                //        'detail': v
                //    });
                $scope.DeliveryProducts.push({
                    'id': v.id, 'name': v.product + ' : ' + v.designname,
                    'qty': v.expectedqty - v.deliveredqty,
                    'detail': v
                });

            });
        }

        $scope.Deliver = function () {

            request = {
                "Item": $scope.TailoringOrder,
                "securitytoken": "String content"
            }

            var srequest = JSON.stringify(request);
            $http({
                method: 'POST',
                url: "TailoringorderService/Deliver",
                data: srequest
            }).success(function (result) {
                if (result != null && result.id > 0) {
                    $rootScope.Toaster.Info("Delivery saved Successfully ");
                    $scope.TailoringOrder = result;

                    $scope.TailoringOrder.deliveredon = new Date();
                    $scope.TailoringOrder.collectedon = new Date();
                    $scope.DeliveryProducts = [];
                    $.each($scope.TailoringOrder.Tailoringitems, function (k, v) {
                        if (v.receivedqty > v.deliveredqty)
                            $scope.DeliveryProducts.push({ 'id': v.id, 'name': v.product + ' : ' + v.designname, 'detail': v });
                    });
                    $scope.CalculateTotal();
                }
                else {
                    $rootScope.Toaster.Error(result.ErrorMessage);
                }
            }).error(function (result) {
            });
        }

        $scope.AddDelivery = function () {
            if (typeof ($scope.TailoringOrder.TailoringDelivery) == 'undefined' ||
                $scope.TailoringOrder.TailoringDelivery == null)
                $scope.TailoringOrder.TailoringDelivery = [];

            var product = $scope.DeliveryProducts.find(function (item) {
                return $scope.TailoringOrder.deliveryproductid == item.id;
            });

            if (typeof (product) == 'undefined' || product == null) {
                $rootScope.Toaster.Info("Please select the item");
                return;
            }
            if (product.qty < $scope.TailoringOrder.delieryqty) {
                $rootScope.Toaster.Info("Delivery qty can't exceed " + product.qty);
                return;
            }

            var deliveryitem = {
                'tailoringitemsid': product.id,
                'tailoringorderid': $scope.TailoringOrder.id,
                'receivedby': $scope.TailoringOrder.receivedcustomer,
                'receivedbycustomer': $scope.TailoringOrder.receivedbycustomer,
                'deliveredqty': $scope.TailoringOrder.delieryqty,
                'deliveredon': $scope.TailoringOrder.deliveredon,
                'product': product.name,
                'id': 0,
                'isactive': true
            };
            $scope.TailoringOrder.TailoringDelivery.push(deliveryitem);
            $scope.LoadDeliveryProducts();
        }

        

        $rootScope.OnLocationInfoReady(function () {
            if (typeof ($rootScope.LocationInfo) != 'undefined' &&
                typeof ($rootScope.LocationInfo.location) != 'undefined') {
                $scope.SearchCondition.locationid = $rootScope.LocationInfo.location.id;
                $scope.SearchCondition.companyid = $rootScope.LocationInfo.company.id;

                $scope.Entity.locationid = $rootScope.LocationInfo.location.id;
                $scope.Entity.companyid = $rootScope.LocationInfo.company.id;

                $scope.TailoringOrder.locationid = $rootScope.LocationInfo.location.id;
                $scope.TailoringOrder.companyid = $rootScope.LocationInfo.company.id;

                $scope.SearchCondition.fromdate = $filter('date')(new Date(), datePicker.shortDisplay);
                $scope.SearchCondition.todate = $filter('date')(new Date(), datePicker.shortDisplay);
                $scope.SearchInbox();
            }
        });

        $scope.system_tailoringunits = false;
        $scope.LoadApplicationSetting = function () {
            if (typeof ($rootScope.UserInfo) != 'undefined' &&
                typeof ($rootScope.UserInfo.appsettings) != 'undefined' &&
                $rootScope.UserInfo.appsettings != null && 
                $rootScope.LocationInfo && 
                $rootScope.LocationInfo.location) {
                $scope.system_supportdeliveryrack = $rootScope.UserInfo.appsettings.system_supportdeliveryrack;
                $scope.system_deliveryrackcard = $rootScope.UserInfo.appsettings.system_deliveryrackcard;
                $scope.system_tailoringunits = $rootScope.UserInfo.appsettings.system_tailoringunits;
                var srequest = JSON.stringify({
                    "Item": { 'locationid': $rootScope.LocationInfo.location.id, 'key': 'APPLICATIONSETTINGS' },
                    "securitytoken": "String content"
                });

                $http({
                    method: 'POST',
                    url: "ConfigurationService/Search",
                    data: srequest
                }).success(function (result) {
                    if (result.length > 0) {
                        $scope.ApplicationSettings = result[0];
                        $scope.sale_customercardprefix = $scope.ApplicationSettings.appsettings.sale_customercardprefix;
                        $scope.sale_deliverycardprefix = $scope.ApplicationSettings.appsettings.sale_deliverycardprefix;
                        $scope.system_tailoringunits = $scope.ApplicationSettings.appsettings.system_tailoringunits;
                    }
                });
            }
            else {
                $timeout(function () {
                    $scope.LoadApplicationSetting();
                }, 50);
            }
        }
        $scope.LoadApplicationSetting();

    }]);

