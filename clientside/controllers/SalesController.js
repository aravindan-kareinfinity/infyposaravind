
var app = angular.module('InfinityLab');

app.controller('SalesController', ['UserDetails', '$scope', '$http', '$location', '$rootScope',
    '$window', 'restService', '$filter', '$timeout', 'localStorageService', '$interval', 'hotkeys', '$stateParams', 'ngDialog',
    function (UserDetails, $scope, $http, $location, $rootScope, $window, restService, $filter, $timeout, localStorageService,
        $interval, hotkeys, $stateParams, ngDialog) {
        $scope.ShowSearch = false;
        $scope.SearchResult = [];
        $scope.lastBills = [];
        $scope.SearchRef = restService.SearchReference;
        $scope.DiscountQueue = false;
        $scope.IsSaleReturn = $stateParams.type == 'SALERETURN';
        $scope.IsBulkSale = $stateParams.type == 'BULKSALE';
        $scope.Title = $scope.IsSaleReturn ? 'Sales Return' : ($scope.IsBulkSale ? 'Sales On Approval' : 'Sales');
        $scope.leftPanStyle = $scope.IsSaleReturn ? '#f7dcdc' : ($scope.IsBulkSale ? '#cff7db' : 'white');
        $scope.IsWarehouse = false;

        $scope.Counters = restService.Get('ReferencelistService/COUNTER', true).then(function (results) {
            $scope.Counters = results;
            $scope.CounterName = $scope.SearchRef($rootScope.LocationInfo.counter.id, results);
        });
        $scope.Locations = restService.Get('ReferencelistService/FULL-LOCATION', true).then(function (results) {
            $scope.Locations = results;
            $scope.LocationName = $scope.SearchRef($rootScope.LocationInfo.location.id, results);
            $.each(results, function (k, v) {
                if (v.id == $rootScope.LocationInfo.location.id)
                {
                    var additionalinfo = v.additionalinfo;
                    if (additionalinfo != null)
                        $scope.IsWarehouse = additionalinfo.iswarehouse;
                    if ($scope.IsWarehouse)
                        $rootScope.Toaster.Error("Sales will not be allowed in Warehouse, Please check the sales location.");
                }                    
            });
        });
        $scope.SalesTax = restService.Get('ReferencelistService/SALESTAX', true).then(function (results) {
            $scope.SalesTax = results;
        });
        $scope.BuyingCompany = restService.Get('ReferencelistService/BUYINGCOMPANY', true).then(function (results) {
            $scope.BuyingCompany = results;
        });
        $scope.Products = restService.Get('ReferencelistService/PRODUCTS', true).then(function (results) {
            $scope.Products = results;
        });
        $scope.ReturnReason = restService.Get('ReferencelistService/RETURNREASON', true).then(function (results) {
            $scope.ReturnReason = results;
        });
        $scope.BulkSaleReason = restService.Get('ReferencelistService/BULKSALEREASON', true).then(function (results) {
            $scope.BulkSaleReason = results;
        });

        $scope.BillType = [{ id: 1, name: 'Cash' }, { id: 2, name: 'Card' }, { id: 3, name: 'Credit' }, { id: 4, name: 'Redemption' }];

        $scope.TemplateSalesDetail = {
            barcode: '',
            printingname: '',
            price: '',
            invoiceitemid: '',
            id: '',
            qty: '',
            isactive: true,
            qtyeditable: false,
            priceeditable: false,
            salesmanid: '',
            salesmanname: '',
            salesmancode: '',
            version: 1
        };

        $scope.TemplateCurrentBill = {
            Billitems: [],
            billTax: [],
            billdate: "",
            billtime: "",
            tax: 0.0,
            id: 0,
            isactive: true,
            iscreditbill: false,
            isbulkreturn: false,
            discount: 0.0,
            receivable: 0.0,
            additionaldiscount: 0.0,
            customerid: 0,
            phoneno: '',
            customername: 'Cash',
            billtype: 0,
            rounding: 0,
            totalqty: 0,
            totalpiece: 0,
            additionaldiscount: 0,
            total: 0,
            totaldiscount: 0,
            rounding: 0
        };

        $scope.AutoSettlement = false;

        if ($rootScope.LocationInfo != null &&
            $rootScope.LocationInfo.location != null &&
            $rootScope.LocationInfo.location.additionalinfo != null &&
            $rootScope.LocationInfo.location.additionalinfo.autosettlement) {
            $scope.AutoSettlement = true;
        };

        $scope.CreateNew = function () {
            $scope.SalesDetail = (JSON.parse(JSON.stringify($scope.TemplateSalesDetail)));
            $scope.CurrentBill = (JSON.parse(JSON.stringify($scope.TemplateCurrentBill)));
            $scope.CurrentBill.billdate = new Date();
            $scope.CurrentBill.billdatedisplay = $filter('date')($scope.CurrentBill.billdate, 'dd-MM-yyyy');
            $scope.CurrentBill.billtimedisplay = $filter('date')($scope.CurrentBill.billdate, 'hh:mm a');
            $scope.additionaldiscountamountrequested = 0;
            $scope.additionaldiscountpercentrequested = 0;
            $scope.autoCoding.code = "";
            $scope.autoCoding.qty = "";
            $scope.autoCoding.lastbit = "";
            $scope.AllowAdditionalDiscount = false;
            $scope.SearchCondition.isbulkreturn = false;
        };

        $scope.ManageBillItem = function (action, item) {

            if (action == "Delete") {
                $scope.CurrentBill.Billitems.splice($scope.CurrentBill.Billitems.indexOf(item), 1);

                $scope.CurrentBill.tax = $scope.CurrentBill.tax - item.tax;
                $scope.CurrentBill.discount = ($scope.CurrentBill.discount - item.discount).round(2);
                $scope.CurrentBill.receivable = ($scope.CurrentBill.receivable - item.receivable).round(2);
                $scope.CurrentBill.rounding = ($scope.CurrentBill.receivable.round(0) - $scope.CurrentBill.receivable).round(2);
                $scope.CurrentBill.totalqty -= (item.qty).round(2);
                $scope.CurrentBill.totalpiece -= (item.piece).round(2);
                $scope.CurrentBill.total = ($scope.CurrentBill.rounding + $scope.CurrentBill.receivable).round(2);


                var existingValue = null;
                angular.forEach($scope.CurrentBill.billTax, function (value, key) {
                    if (value.taxid == item.taxid) {
                        existingValue = value;
                    }
                });

                if (existingValue != null) {
                    existingValue.amount -= ((item.price - item.additionaldiscount) - item.tax);
                    existingValue.tax -= item.tax;
                }
                if ($scope.CurrentBill.Billitems.length == 0)
                    $scope.CurrentBill.isbulkreturn = false;
            }
        };


        $scope.UpdateItemInfo = function (saledetail, recompute) {

            if (typeof (recompute) == 'undefined')
                recompute = false;


            saledetail.qty = $scope.GetNumber(saledetail.qty);
            saledetail.piece = $scope.GetNumber(saledetail.piece);

            if (recompute) {
                $scope.CurrentBill.tax = ($scope.CurrentBill.tax - saledetail.tax).round(2);
                $scope.CurrentBill.discount = ($scope.CurrentBill.discount - saledetail.discount).round(2);
                $scope.CurrentBill.receivable = ($scope.CurrentBill.receivable - saledetail.receivable).round(2);
            }

            saledetail.discount = (saledetail.qty * (saledetail.barcodeDetail.discount + saledetail.barcodeDetail.additionaldiscount)).round(2);

            if (typeof (saledetail.additionaldiscount) == 'undefined' || isNaN(saledetail.additionaldiscount))
                saledetail.additionaldiscount = 0;

            saledetail.baseprice = saledetail.price - saledetail.additionaldiscount;

            if (!$scope.CurrentBill.isinterstatesale) {
                var basePrice = (saledetail.baseprice / (100 + (saledetail.taxpercentage)) * 100);
                var halfTax = (basePrice * (saledetail.taxpercentage / 2) / 100).round(2);
                saledetail.itemtax = (halfTax * 2).round(2);
            }
            else {
                saledetail.itemtax = (saledetail.baseprice / (100 + saledetail.taxpercentage)) * saledetail.taxpercentage;
            }

            saledetail.tax = (saledetail.itemtax * saledetail.qty).round(2);
            saledetail.taxid = saledetail.barcodeDetail.taxid;
            saledetail.rate = (saledetail.baseprice - saledetail.itemtax).round(2);
            saledetail.receivable = (saledetail.baseprice * saledetail.qty).round(2);
            saledetail.stockid = saledetail.barcodeDetail.stockid;
            saledetail.salablegoodsid = saledetail.barcodeDetail.id;
            saledetail.companyid = saledetail.barcodeDetail.companyid;

            $scope.CurrentBill.tax = ($scope.CurrentBill.tax + saledetail.tax).round(2);
            $scope.CurrentBill.discount = ($scope.CurrentBill.discount + saledetail.discount).round(2);
            $scope.CurrentBill.receivable = ($scope.CurrentBill.receivable + saledetail.receivable).round(2);
            $scope.CurrentBill.rounding = ($scope.CurrentBill.receivable.round(0) - $scope.CurrentBill.receivable).round(2);
            $scope.CurrentBill.total = ($scope.CurrentBill.rounding + $scope.CurrentBill.receivable).round(2);
            $scope.CurrentBill.additionaldiscount = ($scope.CurrentBill.additionaldiscount + (saledetail.additionaldiscount * saledetail.qty)).round(2);
        }

        $scope.GetNumber = function (value) {
            if (typeof (value) == 'number')
                return value;

            if (typeof (value) == 'undefined' || value == '')
                return 0

            nvalue = parseFloat(value);
            if (isNaN(nvalue))
                return 0;

            return nvalue;
        };

        $scope.AddBillItem = function () {

            var lastbitValue = Math.abs($scope.GetNumber($scope.autoCoding.lastbit));
            if (lastbitValue != 0) {

                $scope.SalesDetail.journalqty = Math.abs(parseFloat($scope.SalesDetail.qty)).round(2);
                $scope.SalesDetail.qty = lastbitValue;
            }

            if ($scope.IsSaleReturn)
                $scope.SalesDetail.qty = $scope.GetNumber($scope.SalesDetail.qty) * -1;

            $scope.SalesDetail.discount = ($scope.SalesDetail.qty * ($scope.SalesDetail.barcodeDetail.discount + $scope.SalesDetail.barcodeDetail.additionaldiscount)).round(2);

            if (typeof ($scope.SalesDetail.additionaldiscount) == 'undefined' || isNaN($scope.SalesDetail.additionaldiscount))
                $scope.SalesDetail.additionaldiscount = 0;

            $scope.SalesDetail.baseprice = ($scope.SalesDetail.price - $scope.SalesDetail.additionaldiscount);

            if (!$scope.CurrentBill.isinterstatesale) {
                var basePrice = ($scope.SalesDetail.baseprice / (100 + ($scope.SalesDetail.taxpercentage)) * 100);
                var halfTax = (basePrice * ($scope.SalesDetail.taxpercentage / 2) / 100).round(2);
                $scope.SalesDetail.itemtax = (halfTax * 2).round(2);
            }
            else {
                $scope.SalesDetail.itemtax = ($scope.SalesDetail.baseprice / (100 + $scope.SalesDetail.taxpercentage)) * $scope.SalesDetail.taxpercentage;
            }


            $scope.SalesDetail.tax = ($scope.SalesDetail.itemtax * $scope.SalesDetail.qty).round(2);
            $scope.SalesDetail.taxid = $scope.SalesDetail.barcodeDetail.taxid;
            $scope.SalesDetail.rate = ($scope.SalesDetail.baseprice - $scope.SalesDetail.itemtax).round(2);
            $scope.SalesDetail.receivable = ($scope.SalesDetail.baseprice * $scope.SalesDetail.qty).round(2);
            $scope.SalesDetail.stockid = $scope.SalesDetail.barcodeDetail.stockid;
            $scope.SalesDetail.salablegoodsid = $scope.SalesDetail.barcodeDetail.id;
            $scope.SalesDetail.companyid = $scope.SalesDetail.barcodeDetail.companyid;
            $scope.SalesDetail.qty = parseFloat($scope.SalesDetail.qty);
            if (isNaN($scope.SalesDetail.qty))
                $scope.SalesDetail.qty = 0;

            if ($scope.SalesDetail.sellingmode == 3)
                $scope.SalesDetail.piece = 1;
            else
                $scope.SalesDetail.piece = parseInt($scope.SalesDetail.qty);

            $scope.SalesDetail.saleamount = $scope.SalesDetail.qty * $scope.SalesDetail.rate;

            if ($scope.SalesDetail.qty == 0)
            {
                $rootScope.Toaster.Error("Zero Qty Can't be added");
                return;
            }

            $scope.CalcualteTotal($scope.SalesDetail);

            $scope.SalesDetail.additionaldiscount = 0;
            //$scope.CurrentBill.Billitems.push($scope.SalesDetail);
            $scope.CurrentBill.Billitems.splice(0, 0, $scope.SalesDetail);
            $scope.SalesDetail = (JSON.parse(JSON.stringify($scope.TemplateSalesDetail)));
        }

        $scope.SwitchPanel = function () {
            $scope.ShowSearch = !$scope.ShowSearch;
        }

        $scope.CalcualteTotal = function (saleitem) {
            $scope.CurrentBill.totalqty = ($scope.CurrentBill.totalqty + saleitem.qty).round(2);
            $scope.CurrentBill.totalpiece = ($scope.CurrentBill.totalpiece + saleitem.piece).round(2);
            $scope.CurrentBill.tax = ($scope.CurrentBill.tax + saleitem.tax).round(2);
            $scope.CurrentBill.discount = ($scope.CurrentBill.discount + saleitem.discount).round(2);
            $scope.CurrentBill.receivable = ($scope.CurrentBill.receivable + saleitem.receivable).round(2);
            $scope.CurrentBill.additionaldiscount = ($scope.CurrentBill.additionaldiscount + saleitem.additionaldiscount).round(2);
            $scope.CurrentBill.rounding = ($scope.CurrentBill.receivable.round(0) - $scope.CurrentBill.receivable).round(2);
            $scope.CurrentBill.total = ($scope.CurrentBill.rounding + $scope.CurrentBill.receivable).round(2);

            var existingValue = null;

            if (typeof (saleitem.barcodeDetail) == 'undefined')
                saleitem.barcodeDetail = { 'taxid': saleitem.taxid };

            angular.forEach($scope.CurrentBill.billTax, function (value, key) {
                if (existingValue == null && value.taxid == saleitem.barcodeDetail.taxid) {
                    existingValue = value;
                }
            });

            if (existingValue == null) {
                existingValue = {
                    taxid: saleitem.barcodeDetail.taxid,
                    taxName: saleitem.taxname,
                    amount: saleitem.price - saleitem.tax,
                    tax: saleitem.tax,
                    taxpercentage: saleitem.taxpercentage
                }
                $scope.CurrentBill.billTax.push(existingValue)
            }
            else {
                existingValue.amount += (saleitem.price - saleitem.tax);
                existingValue.tax += saleitem.tax;
            }
        };

        $scope.ErrorMessage = "";

        $scope.SearchCondition = {
            salesmanid: "",
            billmastercode: "",
            billno: null
        }

        $scope.SaveRequest = false;

        $scope.Save = function (requestapproval, print) {
            if ($scope.IsWarehouse) {
                $rootScope.Toaster.Error("Please check the sales location.");
                return;
            }
            $scope.CurrentBill.locationid = $rootScope.LocationInfo.location.id;
            $scope.CurrentBill.counterid = $rootScope.LocationInfo.counter.id;
            if ($scope.IsSaleReturn) {
                $scope.CurrentBill.billtype = 8;
            }
            else if ($scope.IsBulkSale) {
                $scope.CurrentBill.billtype = 10;
                $scope.CurrentBill.isbulkreturn = true;
                if(typeof($scope.CurrentBill.customerid) == 'undefinied' || $scope.CurrentBill.customerid == 0)
                {
                    $rootScope.Toaster.Notice('Please select the customer');
                    $scope.ShowCustomerDialog();
                    return;
                }
            }
            else {
                if ($scope.CurrentBill.iscreditbill && (typeof($scope.CurrentBill.customerid) == 'undefinied' || $scope.CurrentBill.customerid == 0)) {
                    $rootScope.Toaster.Notice('Please select the customer');
                    $scope.ShowCustomerDialog();
                    return;
                }
                $scope.CurrentBill.billtype = $scope.CurrentBill.iscreditbill ? 1 : 0;
            }

            if ($scope.CurrentBill.id > 0) {
                $scope.CurrentBill.id = 0;
                if ($scope.CurrentBill.billno != '')
                    $scope.CurrentBill.returnbillno = $scope.CurrentBill.billno;
                $scope.CurrentBill.billno = '';
            }


            var request = {
                "Item": $scope.CurrentBill
            }
            $scope.SaveRequest = true;
            $http({
                method: 'POST',
                url: "BillService/Save",
                data: JSON.stringify(request)
            }).success(function (result) {

                $scope.SaveRequest = false;
                if (result.id > 0) {
                    $scope.ErrorMessage = "Successfully saved";
                    if (print)
                        $scope.Print(result.Bills[0]);

                    if ($scope.AutoSettlement) {
                        $scope.ShowSettlement(result);
                    };

                    if ($scope.DiscountQueue && !$scope.discounttimerenabled && $scope.CurrentBill.additionaldiscount > 0) {
                        $scope.discounttimerenabled = true;
                        $interval($scope.PullDiscountRequest, 30000, 0, true, request);
                    }

                    $scope.AddBillHistory(result, false, true)
                    $scope.CreateNew();


                    if ($scope.DiscountQueue) {
                        $scope.AllowAdditionalDiscount = false;
                    }
                }
                else {
                    $scope.ErrorMessage = result.ErrorMessage;
                }

            }).error(function (result) {
                $scope.ErrorMessage = result;
                $scope.SaveRequest = false;
            });
        }

        $scope.SearchCustomer = function () {
            if ($scope.CurrentBill.phoneno == null || $scope.CurrentBill.phoneno == "") {
                $scope.CurrentBill.customername = "Cash";
                $scope.setFocuscreditbill();
                return;
            }
            restService.Get('CustomerService/PhoneNumber?id=' + $scope.CurrentBill.phoneno, true).then(function (results) {
                $scope.CurrentBill.customername = results.name;
                $scope.CurrentBill.customerid = results.id;
                $scope.setFocuscreditbill();
            });
        }

        $scope.SearchSalesmen = function () {
            if ($scope.SalesDetail.salesmannameLast != null &&
                $scope.SalesDetail.salesmannameLast != "" &&
                $scope.SalesDetail.salesmannameLast == $scope.SalesDetail.salesmanname) {
                $scope.AddBillItem();
                $scope.setFocusBarcode();
                return;
            }

            if ($scope.SalesDetail.salesmanname == null || $scope.SalesDetail.salesmanname == '') {
                $scope.AddBillItem();
                $scope.setFocusBarcode();
            }
            else {
                restService.Get('EmployeeService/Code/' + $scope.UserInfo.OrganizationID + '/' + $scope.SalesDetail.salesmanname, true).then(function (results) {
                    $scope.SalesDetail.salesmancode = $scope.SalesDetail.salesmanname;
                    $scope.SalesDetail.salesmanname = results.name;
                    $scope.SalesDetail.salesmannameLast = results.name;
                    $scope.SalesDetail.salesmanid = results.id;
                });
            }
        }

        $scope.CheckLocation = function () {
            return $rootScope.LocationInfo != null;
        }

        $scope.SearchBarcode = function () {
            if (($scope.SalesDetail.barcode == "0")
                && $scope.CurrentBill.Billitems.length > 0) {
                $scope.setFocusSave();
                return;
            }
            if ($scope.SalesDetail.barcode == null || $scope.SalesDetail.barcode == "") return;

            for (var index = 0; index < $scope.CurrentBill.Billitems.length; index++) {
                if ($scope.CurrentBill.Billitems[index].barcode == $scope.SalesDetail.barcode) {

                    if ($scope.CurrentBill.Billitems[index].qtyeditable) {

                    }
                    else {
                        $scope.ManageBillItem('Delete', $scope.CurrentBill.Billitems[index]);
                    }
                    $scope.SalesDetail.barcode = "";
                    return;
                }
            }

            if (!$scope.CheckLocation())
                return;

            $scope.SalesDetail.printingname = '';
            $scope.SalesDetail.barcodeDetail = null;
            $scope.SalesDetail.qtyeditable = false;
            $scope.SalesDetail.priceeditable = false;



            restService.Get('SalablegoodsService/Barcode/' + $rootScope.LocationInfo.location.id + '/' + $scope.SalesDetail.barcode).then(function (results) {
                if (results.id != 0) {
                    $scope.SalesDetail.stock = (results.iqty - (results.dqty + results.oqty));
                    $scope.SalesDetail.hsncode = results.hsncode;
                    if ($scope.SalesDetail.stock <= 0) {

                        $.confirm({
                            title: 'Duplicate',
                            content: 'This item already sold, do you want to sell it again?',
                            confirmButton: 'Yes',
                            confirmButtonClass: 'btn-info',
                            icon: 'fa fa-question-circle',
                            animation: 'scale',
                            animationClose: 'top',
                            opacity: 0.5,
                            buttons: {
                                formSubmit: {
                                    text: 'Submit',
                                    btnClass: 'btn-blue',
                                    action: function () {
                                        $scope.SalesDetail.stock = 1;
                                        $scope.AddBarcode(results);


                                        $timeout(function () {
                                            $scope.setFocusSalesman();
                                            $scope.$digest();
                                        }, 10);


                                        return true;
                                    }
                                },
                                cancel: function () {
                                    //close
                                },
                            },

                            onContentReady: function () {
                                // bind to events
                                var jc = this;
	jc.$$formSubmit.trigger('focus');
                                this.$content.find('form').on('submit', function (e) {
                                    // if the user submits the form by pressing enter in the field.
                                    e.preventDefault();
                                    jc.$$formSubmit.trigger('click'); // reference the button and click it
                                });
                            }
                        });

                    } else {

                        //search product information
                        //if product.sellingmode != piece > enable qty editable

                        $scope.AddBarcode(results);
                        //results.iqty <= 1 &&
                        if (results.price != 0)
                            $scope.setFocusSalesman();
                    }
                }
            });
        }

        $scope.UpdateTax = function (SalesDetail) {
            if (typeof (SalesDetail.taxInfo) != 'undefined' &&
                    SalesDetail.taxInfo != null) {
                SalesDetail.taxpercentage = SalesDetail.taxInfo.taxpercentage;
                SalesDetail.taxname = SalesDetail.taxInfo.name;
                if (typeof (SalesDetail.additionaldiscount) == 'undefined' || isNaN(SalesDetail.additionaldiscount))
                    SalesDetail.additionaldiscount = 0;

                var baseprice = (SalesDetail.price - SalesDetail.additionaldiscount);
                if (SalesDetail.taxInfo != null) {
                    var taxitem = SalesDetail.taxInfo.taxsplit.findTax(baseprice);
                    SalesDetail.taxpercentage = taxitem.cgst + taxitem.sgst;
                }

                if (!$scope.CurrentBill.isinterstatesale) {
                    var basePrice = (SalesDetail.qty * baseprice / (100 + (SalesDetail.taxpercentage)) * 100);
                    var halfTax = (basePrice * (SalesDetail.taxpercentage / 2) / 100).round(2);
                    $scope.SalesDetail.itemtax = (halfTax * 2).round(2);
                }
                else {
                    SalesDetail.tax = (SalesDetail.qty * baseprice / (100 + SalesDetail.taxpercentage) * SalesDetail.taxpercentage).round(2);
                }
            }
        };

        $scope.findTax = function (amount) {
            if (this.items == null || this.items.length == 0)
                return null;

            if (this.items.length == 1)
                return this.items[0];

            if (isNaN(amount)) amount = 0;
            for (var i = 0; i < this.items.length; i++) {
                var item = this.items[i];
                if (item.fromamount <= amount && item.toamount >= amount)
                    return item;
            }
            return { 'cgst': 0, 'sgst': 0 };
        }

        $scope.AddBarcode = function (results, autofocus) {
            $scope.SalesDetail.barcode = results.barcode;
            $scope.SalesDetail.printingname = results.sellingname;
            $scope.SalesDetail.price = results.price - results.discount;
            $scope.SalesDetail.additionaldiscountpercentage = 0;
            $scope.SalesDetail.discountpercentage = (results.discount / results.price * 100).round(0);
            if (typeof ($scope.SalesDetail.additionaldiscount) == 'undefined' || isNaN($scope.SalesDetail.additionaldiscount))
                $scope.SalesDetail.additionaldiscount = 0;
            $scope.SalesDetail.baseprice = ($scope.SalesDetail.price - $scope.SalesDetail.additionaldiscount);
            $scope.SalesDetail.locationid = $rootScope.LocationInfo.location.id;

            $scope.SalesDetail.qty = $scope.SalesDetail.stock > 0 || !autofocus ? 1 : 0;

            if (results.stockisbit)
                $scope.SalesDetail.qty = results.stockqty;

            if ($scope.IsSaleReturn) {
                $scope.SalesDetail.qty = $scope.SalesDetail.qty * 1;
            }

            $scope.SalesDetail.companyid = results.companyid;
            $scope.SalesDetail.discount = results.discount;
            $scope.SalesDetail.tax = 0;
            $scope.SalesDetail.taxpercentage = 0;
            $scope.SalesDetail.taxid = results.taxid;
            $scope.SalesDetail.receivable = $scope.SalesDetail.baseprice * $scope.SalesDetail.qty;
            $scope.SalesDetail.salerate = results.price;

            if (results.taxid > 0) {
                $scope.SalesDetail.taxInfo = $scope.SalesTax.find(function (arritem) {
                    return arritem.id == results.taxid;
                });

                if ($scope.SalesDetail.taxInfo != null)
                    $scope.SalesDetail.taxInfo.taxsplit.findTax = $scope.findTax;
                results.taxname = $scope.SalesDetail.taxname;
                $scope.UpdateTax($scope.SalesDetail);
            }
            $scope.ProductDetails = [];

            $.each($scope.Products, function (k, v) {
                if (v.id == results.productid)
                    $scope.ProductDetails.push(v);
            });


            if (!results.stockisbit && $scope.ProductDetails[0].sellingmode > 1 && $scope.SalesDetail.stock > 0) {
                $scope.SalesDetail.qtyeditable = true;
                $scope.SalesDetail.sellingmode = $scope.ProductDetails[0].sellingmode;
                if (autofocus) $scope.setFocusQty();
            }

            if (results.price == 0) {
                $scope.SalesDetail.priceeditable = true;
                if (autofocus) $scope.setFocusPrice();
            }

            $scope.SalesDetail.barcodeDetail = results;
        }

        $scope.ManageSearchItem = function (action, item) {
            if (action == "Edit") {
                $scope.ShowSearch = false;
                $scope.EditBill(item);
            }
        };

        $scope.RequestDiscount = function () {
            $.confirm({
                title: 'Reason',
                content: '' +
                '<form action="" class="formName">' +
                '<div class="form-group">' +
                '<input type="text" placeholder="reason" class="name form-control" required />' +
                '</div>' +
                '</form>',
                buttons: {
                    formSubmit: {
                        text: 'OK',
                        btnClass: 'btn-blue',
                        action: function () {
                            var name = this.$content.find('.name').val();
                            if (!name) {
                                $.alert('provide a valid name');
                                return false;
                            }
                            $scope.CurrentBill.discountrequestreason = name;
                            $scope.Save(true, false);
                            return true;
                        }
                    },
                    cancel: function () {
                        //close
                    },
                },
                onContentReady: function () {
                    // bind to events
                    var jc = this;
	jc.$$formSubmit.trigger('focus');
                    this.$content.find('form').on('submit', function (e) {
                        // if the user submits the form by pressing enter in the field.
                        e.preventDefault();
                        jc.$$formSubmit.trigger('click'); // reference the button and click it
                    });
                }
            });
        };

        $scope.RejectDiscount = function (item) {
            $.confirm({
                title: 'Reason for rejection',
                content: '' +
                '<form action="" class="formName">' +
                '<div class="form-group">' +
                '<input type="text" placeholder="reason" class="name form-control" required />' +
                '</div>' +
                '</form>',
                buttons: {
                    formSubmit: {
                        text: 'Reject',
                        btnClass: 'btn-blue',
                        action: function () {
                            var name = this.$content.find('.name').val();
                            if (!name) {
                                return false;
                            }
                            $scope.ChangeApproveStatus(item, 'R', name);
                            return true;
                        }
                    },
                    cancel: function () {
                        //close
                    },
                },
                onContentReady: function () {
                    // bind to events
                    var jc = this;
	jc.$$formSubmit.trigger('focus');
                    this.$content.find('form').on('submit', function (e) {
                        // if the user submits the form by pressing enter in the field.
                        e.preventDefault();
                        jc.$$formSubmit.trigger('click'); // reference the button and click it
                    });
                }
            });
        };

        $scope.Cancel = function (item) {
                request = {
                    "item": {
                        'ids': [item.id], 'locationid': $rootScope.LocationInfo.location.id,
                        'isbulkreturn': item.isbulkreturn,
                        'isbulksale': $scope.IsBulkSale
                    },
                    "securitytoken": "String content"
                }

                var srequest = JSON.stringify(request);

                $http({
                    method: 'POST',
                    url: "BillService/Cancellation",
                    data: srequest
                }).success(function (result) {
                    $scope.CreateNew();
                    $rootScope.Toaster.Notice('Cancellation completed, Bill No - ' + result[0].billno);
                }).error(function (result) {
                });
        };

        $scope.Print = function (item) {
            if (typeof (item.printdocument) != 'undefined' &&
                item.printdocument != null && item.printdocument.length > 0) {
                $scope.Sent2Printer(item.printdocument);
                return;
            }

            var request = {
                "Item": { 'id': typeof (item.billid) != 'undefined' ? item.billid : item.id, 'enableprintdocument': true, 'billtype': item.billtype, 'isbulkreturn': item.billtype == 10 },
                "SecurityToken": "String content"
            }

            var srequest = JSON.stringify(request);

            $http({
                method: 'POST',
                url: "BillService/Search",
                data: srequest
            }).success(function (result) {

                if (result != null && result.length == 1) {
                    $scope.Sent2Printer(result[0].printdocument);
                }
            });
        };

        $scope.Sent2Printer = function (printdocument) {
            $rootScope.Sent2Printer(printdocument, printerData);
            //var printerData = localStorageService.get('PrinterData');
            //if (printerData == null) {
            //    return;
            //}
            //var request = {
            //    'id': printerData.printerid,
            //    'content': printdocument,
            //    'encoded': 'BASE64'
            //};
            //var srequest = JSON.stringify(request);
            //$http({
            //    method: 'POST',
            //    url: printerData.service,
            //    data: srequest
            //}).success(function (result) {
            //});
        }

        $scope.EditBill = function (item) {
            $scope.SearchCondition.billno = item.billno;
            $scope.SearchCondition.billid = item.id;
            $scope.SearchBill();
        }

        $scope.SearchBill = function (callback) {
            request = {
                "Item": {
                    'billno': $scope.SearchCondition.billno,
                    'billid': $scope.SearchCondition.billid,
                    'isbulkreturn': $scope.SearchCondition.isbulkreturn,
                    'locationid': $rootScope.LocationInfo.location.id
                },
                "SecurityToken": "String content"
            }

            var srequest = JSON.stringify(request);

            $http({
                method: 'POST',
                url: "BillService/Search",
                data: srequest
            }).success(function (result) {


                var bill = result[0];
                if (typeof (callback) != 'undefined')
                    callback(bill);

                if ($scope.IsSaleReturn && bill.billtype == 9)
                    return;

                $scope.CurrentBill = bill;
                $scope.additionaldiscountamountrequested = bill.additionaldiscount;
                $scope.CurrentBill.Billitems = bill.Billitems;
                $scope.CurrentBill.billTax = bill.BillTax;
                $scope.CurrentBill.billdatedisplay = $filter('date')($scope.CurrentBill.billdate, 'dd-MM-yyyy');
                $scope.CurrentBill.billtimedisplay = $filter('date')($scope.CurrentBill.billdate, 'hh:mm a');
                $.each(bill.BillTax, function (k, v) {
                    var tax = $scope.SalesTax.find(function (item) {
                        return item.id == v.taxid;
                    });
                    v.taxName = tax != null ? tax.name : '';
                });

                if ($scope.IsSaleReturn) {
                    $scope.ReverseBillValue();
                    $scope.CurrentBill.returnbillno = $scope.CurrentBill.billno;
                    $scope.CurrentBill.billno = '';
                    $scope.CurrentBill.id = 0;
                    $scope.autoCoding.code = '';
                    $scope.AllowAdditionalDiscount = ($scope.CurrentBill.additionaldiscount > 0);
                }

            }).error(function (result) {
            });
        };

        $scope.ReverseBillValue = function () {
            $scope.CurrentBill.totalqty = 0;
            $scope.CurrentBill.totalpiece = 0;
            $scope.CurrentBill.tax = 0;
            $scope.CurrentBill.discount = 0;
            $scope.CurrentBill.receivable = 0;
            $scope.CurrentBill.rounding = 0;
            $scope.CurrentBill.total = 0;
            $scope.CurrentBill.additionaldiscount = 0;
            $scope.CurrentBill.billtype = 9;

            for (var i = 0; i < $scope.CurrentBill.Billitems.length; i++) {
                $scope.CurrentBill.Billitems[i].qty = Math.abs($scope.CurrentBill.Billitems[i].qty) * -1;
                $scope.CurrentBill.Billitems[i].piece = Math.abs($scope.CurrentBill.Billitems[i].piece) * -1;
                $scope.CurrentBill.Billitems[i].tax = Math.abs($scope.CurrentBill.Billitems[i].tax) * -1;
                $scope.CurrentBill.Billitems[i].discount = Math.abs($scope.CurrentBill.Billitems[i].discount) * -1;
                $scope.CurrentBill.Billitems[i].receivable = Math.abs($scope.CurrentBill.Billitems[i].receivable) * -1;
                $scope.CurrentBill.Billitems[i].additionaldiscount = Math.abs($scope.CurrentBill.Billitems[i].additionaldiscount) * -1;
                $scope.CurrentBill.Billitems[i].saleamount = $scope.CurrentBill.Billitems[i].qty * $scope.CurrentBill.Billitems[i].rate;
                $scope.CalcualteTotal($scope.CurrentBill.Billitems[i]);
            }
        }

        $scope.Search = function () {

            request = {
                "Item": $scope.SearchCondition,
                "SecurityToken": "String content"
            }

            var srequest = JSON.stringify(request);

            $http({
                method: 'POST',
                url: "BillService/Search",
                data: srequest
            }).success(function (result) {
                $scope.SearchResult = result;
            }).error(function (result) {
            });

        };


        $scope.ChangeApproveStatus = function (item, status, reason) {
            var request = null;
            if (status == 'A') {
                request = {
                    "Item": { 'id': item.id, 'status': 3 },
                    "SecurityToken": "String content"
                }
            } else if (status == 'R') {
                request = {
                    "Item": { 'id': item.id, 'status': 2, 'actionreason': reason },
                    "SecurityToken": "String content"
                }
            } else if (status == 'X') {
                request = {
                    "Item": { 'id': item.id, 'hasaccepted': true },
                    "SecurityToken": "String content"
                }
            }

            var srequest = JSON.stringify(request);

            $http({
                method: 'POST',
                url: "DiscountqueueService/Save",
                data: srequest
            }).success(function (result) {
                $scope.SearchResult = result;
            }).error(function (result) {
            });
        }

        $scope.AddBillHistory = function (result, isdiscount, isupdate) {
            if (typeof ($scope.lastBills) == 'undefined' || $scope.lastBills == null)
                $scope.lastBills = [];
            if (isdiscount) {
                $.each(result, function (k, v) {
                    var existbill = $scope.lastBills.find(function (item) {
                        return item.type == 'B' && item.id == v.billid;
                    });
                    if (existbill != null && existbill.id > 0) {
                        existbill.discountstatus = v.discountstatus;
                    }
                    else {
                        var existbill = $scope.lastBills.find(function (item) {
                            return item.type == 'D' && item.id == v.id;
                        });
                        if (existbill != null && existbill.id > 0) {
                            existbill.discountstatus = v.status;
                        }
                        else {
                            v.type = 'D';
                            v.discountstatus = v.status;
                            $scope.lastBills.push(v);
                        }
                    }
                })
            }
            else {

                $scope.lastBills = [];
                var listofnormalbill = [];
                if (typeof ($scope.lastBills) == 'undefined' || $scope.lastBills == null)
                    $scope.lastBills = [];

                $.each($scope.lastBills, function (k, v) {
                    if (v.type == 'B' && v.discountstatus == 0)
                        listofnormalbill.push(v);
                    if (v.type == 'D' && isupdate && result.id == v.billid)
                        listofnormalbill.push(v);
                });


                $.each(listofnormalbill, function (k, v) {
                    $scope.lastBills.splice($scope.lastBills.indexOf(v), 1);
                });

                $.each(result.Bills, function (k, v) {
                    v.type = 'B';
                    $scope.lastBills.push(v);
                });
            }
        }

        $scope.discounttimerenabled = false;
        $scope.CheckDiscountRequest = function () {
            var request = {
                "Item": {
                    'requestedby': 1,
                    'mode': $scope.CanApprove ? 2 : 1
                },
                "SecurityToken": "String content"
            }

            var srequest = JSON.stringify(request);

            $http({
                method: 'POST',
                url: "DiscountqueueService/Search",
                data: srequest
            }).success(function (result) {
                if (result.length > 0) {
                    request.Item.fromid = result[result.length - 1].id;
                    $scope.AddBillHistory(result, true, false);
                    if ($scope.DiscountQueue) {
                        $scope.discounttimerenabled = true;
                        $interval($scope.PullDiscountRequest, 30000, 0, true, request);
                    }
                }
            });
        }

        if ($scope.DiscountQueue) {
            $scope.CheckDiscountRequest();
        }

        $scope.PullDiscountRequest = function (request) {
            var srequest = JSON.stringify(request);

            $http({
                method: 'POST',
                url: "DiscountqueueService/Search",
                data: srequest
            }).success(function (result) {
                if (result.length > 0) {
                    request.Item.fromid = result[result.length - 1].id;
                    $scope.AddBillHistory(result, true, false);
                }
            });
        }

        $scope.CanApprove = true;
        $scope.AllowAdditionalDiscount = false;
        $scope.ShowDiscountApprove = false;
        $scope.ShowDiscountQueue = function (enable) {

        };

        //$scope.ApplyDiscount = function (ispercentage) {
        //    if (ispercentage) {
        //        var percentage = parseFloat($scope.additionaldiscountpercentrequested);
        //        if (percentage < 0 || percentage > 100)
        //            percentage = 0;
        //        discountamount = 0;
        //        var tax = 0;
        //        $.each($scope.CurrentBill.Billitems, function (k, v) {

        //            var existingTaxValue = $scope.CurrentBill.billTax.find(function (item) {
        //                return item.taxid == v.taxid;
        //            });

        //            existingTaxValue.amount -= ((v.rate - v.additionaldiscount) * v.qty);
        //            existingTaxValue.tax -= (v.tax);

        //            v.additionaldiscount = Math.round(v.price * percentage / 100, 0);
        //            v.additionaldiscountpercentage = percentage;
        //            discountamount += v.additionaldiscount;
        //            $scope.UpdateTax(v);
        //            tax += v.tax;

        //            existingTaxValue.amount += ((v.rate - v.additionaldiscount) * v.qty);
        //            existingTaxValue.tax += (v.tax);

        //        });
        //        $scope.CurrentBill.additionaldiscount = discountamount;
        //        $scope.additionaldiscountamountrequested = discountamount;
        //        $scope.CurrentBill.tax = tax;
        //    }
        //    else {
        //        var discountamount = parseFloat($scope.additionaldiscountamountrequested);
        //        if (discountamount < 0)
        //            discountamount = 0;
        //        $scope.CurrentBill.additionaldiscount = discountamount;
        //        percentage = ((discountamount / $scope.CurrentBill.receivable) * 100);
        //        var maxitem = null;
        //        var tax = 0;
        //        $.each($scope.CurrentBill.Billitems, function (k, v) {
        //            if (maxitem == null || maxitem.price < v.price)
        //                maxitem = v;

        //            var existingTaxValue = $scope.CurrentBill.billTax.find(function (item) {
        //                return item.taxid == v.taxid;
        //            });

        //            existingTaxValue.amount -= ((v.rate - v.additionaldiscount) * v.qty);
        //            existingTaxValue.tax -= (v.tax);

        //            v.additionaldiscount = Math.round(v.price * percentage / 100, 0);
        //            v.additionaldiscountpercentage = percentage;
        //            discountamount -= v.additionaldiscount;
        //            $scope.UpdateTax(v);

        //            existingTaxValue.amount += ((v.rate - v.additionaldiscount) * v.qty);
        //            existingTaxValue.tax += (v.tax);
        //            tax += v.tax;
        //        });

        //        if (discountamount < 0)
        //            maxitem.additionaldiscount -= discountamount;
        //        else
        //            maxitem.additionaldiscount += discountamount;
        //        //maxitem.receivable = (maxitem.price - maxitem.additionaldiscount) * maxitem.qty;
        //        $scope.CurrentBill.tax = tax;
        //        $scope.additionaldiscountpercentrequested = (percentage).round(2);
        //    }
        //}

        $scope.autoCoding = { 'code': '' };

        $scope.ValidateReturnAndSave = function () {
            if (($scope.CurrentBill.billtype == 8 || $scope.CurrentBill.billtype == 9) &&
                $scope.CurrentBill.receivable < 0) {
                $scope.Save(false, true);
            }
        }

        $scope.ShowCancellDialog = function (bill) {
            $rootScope.Confirm('Delete Bulk Bill','Do you want to cancel the bulk bill - '+bill.billno,function()
            {
                bill.isbulkreturn = true;
                var request = {
                    "Item": bill
                }
                $http({
                    method: 'POST',
                    url: "BillService/Delete",
                    data: JSON.stringify(request)
                }).success(function (result) {
                    $scope.CreateNew();
                });
            });
        };

        $scope.SearchAutoCoding = function () {

            if ($scope.autoCoding.code == null || $scope.autoCoding.code == "") return;

            if (($scope.autoCoding.code == "0")
                            && $scope.CurrentBill.Billitems.length > 0) {

                if ($scope.IsSaleReturn) {
                    $scope.setFocusreturnbillno();
                }
                else {
                    $scope.Save(false, true);
                    $scope.autoCoding.code = '';
                }
                return;
            }

            if ($scope.autoCoding.code == "P" || $scope.autoCoding.code == "p") {
                if ($scope.CurrentBill.id > 0) {
                    $scope.Print($scope.CurrentBill);
                    $scope.autoCoding.code = '';
                }
                return;
            }

            if ($scope.autoCoding.code == "R" || $scope.autoCoding.code == "r") {
                if (!$scope.IsSaleReturn)
                {
                    $rootScope.Toaster.Notice('Please use sales return screen for returns.');
                    return;
                }

                $scope.ReverseBillValue();
                $scope.autoCoding.code = '';
                $scope.setFocusreturnbillno();
                return;
            }


            if ($scope.autoCoding.code == "m" || $scope.autoCoding.code == "M") {
                $scope.autoCoding.code = '';
                $scope.CurrentBill.id = 0;
                $scope.CurrentBill.billno = '';
                return;
            }

            if ($scope.autoCoding.code == "d" || $scope.autoCoding.code == "D") {
                $scope.autoCoding.code = '';
                $scope.ShowAdditionalDiscount();
                return;
            }

            if ($scope.autoCoding.code == "-"
                            && $scope.CurrentBill.Billitems.length > 0) {
                $scope.ManageBillItem('Delete', $scope.CurrentBill.Billitems[0]);
                $scope.autoCoding.code = '';
                return;
            }

            if ($scope.autoCoding.code == "*") {
                //while ($scope.CurrentBill.Billitems.length > 0)
                //    $scope.ManageBillItem('Delete', $scope.CurrentBill.Billitems[0]);
                //$scope.autoCoding.code = '';
                $scope.CreateNew();
                return;
            }

            if ($scope.autoCoding.code == "N" || $scope.autoCoding.code == "n") {
                $scope.CreateNew();
                return;
            }

            if ($scope.autoCoding.code.length == 10) {
                var letters = /^[0-9]+$/;
                if ($scope.autoCoding.code.match(letters)) {
                    $scope.CurrentBill.phoneno = $scope.autoCoding.code;
                    $scope.autoCoding.code = '';
                    $scope.SearchCustomer();
                    return;
                }
            }

            if ($scope.autoCoding.code == 'C' || $scope.autoCoding.code == 'c') {
                $scope.ShowCustomerDialog();
                return;
            }

            if ($scope.autoCoding.code.startsWith('.') && $scope.autoCoding.code.length > 0) {
                restService.Get('EmployeeService/Code/' + $scope.UserInfo.OrganizationID + '/' + $scope.autoCoding.code.replace('.', ''), true).then(function (results) {
                    if (results.id == 0) return;

                    if (typeof ($scope.CurrentBill.Billitems[0].salesmanid) == 'undefined' ||
                       $scope.CurrentBill.Billitems[0].salesmanid == null) {
                        for (var i = 0; i < $scope.CurrentBill.Billitems.length; i++) {
                            if (typeof ($scope.CurrentBill.Billitems[i].salesmanid) == 'undefined' ||
                                     $scope.CurrentBill.Billitems[i].salesmanid == null) {
                                $scope.CurrentBill.Billitems[i].salesmanname = results.name;
                                $scope.CurrentBill.Billitems[i].salesmanid = results.id;
                                $scope.autoCoding.code = '';
                            }
                        }
                    }
                    else {
                        var replacesmid = $scope.CurrentBill.Billitems[0].salesmanid;
                        for (var i = $scope.CurrentBill.Billitems.length - 1; i >= 0; i--) {
                            if ($scope.CurrentBill.Billitems[i].salesmanid == replacesmid) {
                                $scope.CurrentBill.Billitems[i].salesmanname = results.name;
                                $scope.CurrentBill.Billitems[i].salesmanid = results.id;
                                $scope.autoCoding.code = '';
                            }
                        }
                    }
                });
                return;
            }

            if ($scope.billprefix != null && $scope.autoCoding.code.startsWith($scope.billprefix)) {
                $scope.SearchCondition.billno = $scope.autoCoding.code;
                $scope.SearchCondition.billid = 0;
                $scope.SearchCondition.isbulkreturn = false;
                $scope.SearchBill(function (bill) {
                    $scope.autoCoding.code = "";
                    $scope.autoCoding.qty = "";
                    $scope.autoCoding.lastbit = "";
                    $scope.CurrentBill.isbulkreturn = false;
                    $scope.CurrentBill.returnbillno = bill.billno;
                    bill.returnbillno = bill.billno;
                    var oldbill = $scope.lastBills.find(function (item) {
                        return item.id == bill.id;
                    });

                    if (typeof (oldbill) == 'undefined' || oldbill == null)
                        $scope.lastBills.push((JSON.parse(JSON.stringify(bill))));

                    $scope.setFocusautocoding();
                });
                return;
            } else if ($scope.bulkbillprefix != null && $scope.autoCoding.code.startsWith($scope.bulkbillprefix)) {
                $scope.SearchCondition.billno = $scope.autoCoding.code;
                $scope.SearchCondition.billid = 0;
                $scope.SearchCondition.isbulkreturn = true;
                $scope.SearchBill(function (bill) {
                    $scope.autoCoding.code = "";
                    $scope.autoCoding.qty = "";
                    $scope.autoCoding.lastbit = "";
                    $scope.CurrentBill.isbulkreturn = true;
                    $scope.CurrentBill.returnbillno = bill.billno;
                    $scope.CurrentBill.billno = bill.billno;
                    bill.returnbillno = bill.billno;
                    bill.isbulkreturn = true;
                   
                    $scope.setFocusautocoding();

                    var oldbill = $scope.lastBills.find(function (item) {
                        return item.id == bill.id;
                    });

                    if (typeof (oldbill) == 'undefined' || oldbill == null)
                        $scope.lastBills.push((JSON.parse(JSON.stringify(bill))));

                    /*if ($scope.IsBulkSale) {
                        $scope.ShowCancellDialog(bill);
                    };*/
                });
                return;
            }
            else if ($scope.CurrentBill.id > 0) {
                $scope.SalesDetail = (JSON.parse(JSON.stringify($scope.TemplateSalesDetail)));
                $scope.CurrentBill = (JSON.parse(JSON.stringify($scope.TemplateCurrentBill)));
                $scope.CurrentBill.billdate = new Date();
            }

            for (var index = 0; index < $scope.CurrentBill.Billitems.length; index++) {
                if ($scope.CurrentBill.Billitems[index].barcode == $scope.autoCoding.code &&
                    (typeof ($scope.CurrentBill.Billitems[index].salesmanid) == 'undefined' || $scope.CurrentBill.Billitems[index].salesmanid == 0)) {

                    if ($scope.CurrentBill.Billitems[index].sellingmode == 1) {
                        if (typeof ($scope.CurrentBill.Billitems[index].stock) != 'undefined' && $scope.CurrentBill.Billitems[index].stock <= 1) {
                            $scope.ManageBillItem('Delete', $scope.CurrentBill.Billitems[index]);
                            $scope.autoCoding.code = "";
                            $rootScope.Toaster.Notice('Deleted');
                            return;
                        }
                        else
                        {

                            var newQty = $scope.IsSaleReturn ? -1 : 1;

                            $scope.CurrentBill.Billitems[index].qty += newQty;
                            $scope.CurrentBill.totalqty = ($scope.CurrentBill.totalqty + newQty).round(2);
                            $scope.CurrentBill.totalpiece = ($scope.CurrentBill.totalpiece - $scope.CurrentBill.Billitems[index].piece).round(2);
                            if ($scope.CurrentBill.Billitems[index].sellingmode == 3) {
                                $scope.CurrentBill.Billitems[index].piece = $scope.GetNumber($scope.CurrentBill.Billitems[index].piece) + newQty;
                            }
                            else {
                                $scope.CurrentBill.Billitems[index].piece = $scope.GetNumber($scope.CurrentBill.Billitems[index].qty);
                            }
                            $scope.CurrentBill.totalpiece = ($scope.CurrentBill.totalpiece + $scope.CurrentBill.Billitems[index].piece).round(2);
                            $scope.UpdateItemInfo($scope.CurrentBill.Billitems[index], true);
                            $scope.autoCoding.code = "";
                            $rootScope.Toaster.Notice('Qty increased to ' + $scope.CurrentBill.Billitems[index].piece);

                            if ($scope.CurrentBill.Billitems[index].piece == 3) {
                                $rootScope.Prompt('Multiple', 'No Of Pieces', function (count) {

                                    var totalcount = $rootScope.GetNumber(count);
                                    if (totalcount > 0) {

                                        $scope.CurrentBill.totalqty = ($scope.CurrentBill.totalqty - $scope.CurrentBill.Billitems[index].qty).round(2);
                                        $scope.CurrentBill.totalpiece = ($scope.CurrentBill.totalpiece - $scope.CurrentBill.Billitems[index].piece).round(2);

                                        $scope.CurrentBill.Billitems[index].qty = totalcount;
                                        $scope.CurrentBill.Billitems[index].piece = totalcount;

                                        $scope.CurrentBill.totalpiece = ($scope.CurrentBill.totalpiece + $scope.CurrentBill.Billitems[index].piece).round(2);
                                        $scope.CurrentBill.totalqty = ($scope.CurrentBill.totalqty + $scope.CurrentBill.Billitems[index].qty).round(2);

                                        $scope.UpdateItemInfo($scope.CurrentBill.Billitems[index], true);

                                        $rootScope.Toaster.Notice('Qty altered to ' + $scope.CurrentBill.Billitems[index].piece);
                                    }
                                });
                            }
                            return;
                        }
                    }
                    if ($scope.CurrentBill.Billitems[index].sellingmode == 2) {

                        var newQty = $scope.IsSaleReturn ? -1 : 1;

                        $scope.CurrentBill.Billitems[index].qty += newQty;
                        $scope.CurrentBill.totalqty = ($scope.CurrentBill.totalqty + newQty).round(2);
                        $scope.CurrentBill.totalpiece = ($scope.CurrentBill.totalpiece - $scope.CurrentBill.Billitems[index].piece).round(2);

                        if ($scope.CurrentBill.Billitems[index].sellingmode == 3) {
                            $scope.CurrentBill.Billitems[index].piece = $scope.GetNumber($scope.CurrentBill.Billitems[index].piece) + newQty;
                        }
                        else {
                            $scope.CurrentBill.Billitems[index].piece = parseInt($scope.CurrentBill.Billitems[index].qty);
                        }
                        $scope.CurrentBill.totalpiece = ($scope.CurrentBill.totalpiece + $scope.CurrentBill.Billitems[index].piece).round(2);
                        $scope.UpdateItemInfo($scope.CurrentBill.Billitems[index], true);
                        $scope.autoCoding.code = "";
                        $rootScope.Toaster.Notice('Qty increased to ' + $scope.CurrentBill.Billitems[index].qty);


                        if ($scope.CurrentBill.Billitems[index].piece == 3) {
                            $rootScope.Prompt('Multiple', 'No Of Pieces', function (count) {

                                var totalcount = $rootScope.GetNumber(count);
                                if (totalcount > 0) {

                                    $scope.CurrentBill.totalqty = ($scope.CurrentBill.totalqty - $scope.CurrentBill.Billitems[index].qty).round(2);
                                    $scope.CurrentBill.totalpiece = ($scope.CurrentBill.totalpiece - $scope.CurrentBill.Billitems[index].piece).round(2);

                                    $scope.CurrentBill.Billitems[index].qty = totalcount;
                                    $scope.CurrentBill.Billitems[index].piece = totalcount;

                                    $scope.CurrentBill.totalpiece = ($scope.CurrentBill.totalpiece + $scope.CurrentBill.Billitems[index].piece).round(2);
                                    $scope.CurrentBill.totalqty = ($scope.CurrentBill.totalqty + $scope.CurrentBill.Billitems[index].qty).round(2);

                                    $scope.UpdateItemInfo($scope.CurrentBill.Billitems[index], true);

                                    $rootScope.Toaster.Notice('Qty altered to ' + $scope.CurrentBill.Billitems[index].piece);
                                }
                            });
                        }
                        return;
                    }
                }
            }

            $scope.SalesDetail.printingname = '';
            $scope.SalesDetail.barcodeDetail = null;
            $scope.SalesDetail.qtyeditable = false;
            $scope.SalesDetail.priceeditable = false;

            restService.Get('SalablegoodsService/Barcode/' + $rootScope.LocationInfo.location.id + '/' + $scope.autoCoding.code).then(function (results) {
                if (results.id != 0) {
                    $scope.SalesDetail.barcode = results.barcode;
                    $scope.SalesDetail.stock = (results.iqty - (results.dqty + results.oqty));
                    $scope.SalesDetail.hsncode = results.hsncode;
                    $scope.AddBarcode(results, false);

                    var product = $scope.Products.find(function (item) {
                        return item.id == results.productid;
                    });

                    $scope.SalesDetail.sellingmode = product.sellingmode;

                    if ($scope.SalesDetail.sellingmode == 3 && !results.stockisbit) {
                        $scope.setFocusautocodingqty();
                    }
                    else {
                        $scope.AddBillItem();
                        $scope.autoCoding.code = "";
                        $scope.autoCoding.qty = "";
                        $scope.autoCoding.lastbit = "";
                        $scope.setFocusautocoding();
                    }
                }
            });
        };

        $scope.CreateNew();

        $scope.CheckCreditFocus = function () {
            if (!$scope.CurrentBill.iscreditbill &&
                $scope.CurrentBill.customername != null &&
                $scope.CurrentBill.customername.length > 0) {
                $scope.setFocusautocoding();
            }
            else {
                $scope.setFocuscustomername();
            }
        }

        $scope.LastBiteEnter = function () {
            var islastbit = false;
            if ($scope.autoCoding.qty.toString().indexOf('-') > 0) {
                var bitinfo = $scope.autoCoding.qty.split('-');
                var qty = $scope.GetNumber(bitinfo[0]);
                var lastbit = $scope.GetNumber(bitinfo[1]);
                if (qty < lastbit) {
                    $rootScope.Toaster.Error('last bit discount cant exceed actual');
                    return;
                }
                var journalqty = qty - lastbit;
                $scope.autoCoding.lastbit = journalqty.toString();
                $scope.autoCoding.qty = qty.toString();
                islastbit = true;
            }

            var bqty = Math.abs($scope.GetNumber($scope.autoCoding.qty));
            $scope.SalesDetail.qty = bqty;
            $scope.AddBillItem();
            $scope.autoCoding.code = "";
            $scope.autoCoding.qty = "";
            $scope.autoCoding.lastbit = "";
            if (islastbit)
                $rootScope.Toaster.Notice('last bit added');
            $scope.setFocusautocoding();
        };

        hotkeys.add('alt+p', 'Print', function (event) {
            event.preventDefault();
            $scope.Print($scope.CurrentBill);
        });

        $scope.ShowCustomerDialog = function () {
            var dialog = ngDialog.open({
                template: 'SearchCustomer',
                data: [$scope],
                width: '800px',
                controller: ['$scope', '$timeout', '$http', function ($scope, $timeout, $http) {
                    $scope.PopupList = [];
                    $scope.Customer = null;
                    $scope.Select = function () {
                        $scope.Customer = null;
                        $.each($scope.PopupList, function (k, v) {
                            if (v.selected)
                                $scope.Customer = v;
                        });
                        if ($scope.Customer != null)
                            $scope.closeThisDialog($scope.Customer);
                    };
                    $scope.Create = function () {
                        var request = {
                            "Item": { 'name': $scope.name, 'area': $scope.area, 'mobileno': $scope.contactno, 'gstno': $scope.gstno, 'hasgst': $scope.hasgst },
                            "SecurityToken": "String content"
                        }
                        var srequest = JSON.stringify(request);
                        $http({
                            method: 'POST',
                            url: "CustomerService/Save",
                            data: srequest
                        }).success(function (result) {
                            $scope.closeThisDialog(result);
                        });
                    };
                    $scope.Search = function () {
                        var request = {
                            "Item": { 'name': $scope.name, 'area': $scope.area, 'mobileno': $scope.contactno, 'gstno': $scope.gstno, 'hasgst': $scope.hasgst },
                            "SecurityToken": "String content"
                        }
                        var srequest = JSON.stringify(request);
                        $http({
                            method: 'POST',
                            url: "CustomerService/Search",
                            data: srequest
                        }).success(function (result) {
                            $scope.PopupList = result;
                        });
                    };
                }],
                className: 'ngdialog-theme-plain'
            });
            dialog.closePromise.then(function (data) {
                {
                    if (typeof (data.value) != 'undefined' && data.value != null && data.value != '$escape' && data.value != '$document' && data.value.id > 0) {
                        $scope.CurrentBill.customerid = data.value.id;
                        $scope.CurrentBill.code = '';
                        $scope.CurrentBill.phoneno = data.value.mobileno;
                        $scope.CurrentBill.customername = data.value.name;
                    }
                    $scope.autoCoding.code = '';
                    $scope.setFocusautocoding();
                }
            });
        }

        $scope.ShowAdditionalDiscount = function () {
            var list = [];
            $.each($scope.CurrentBill.Billitems, function (k, v) {
                list.push({
                    'core': v, 'additionaldiscount': v.additionaldiscount, 'additionaldiscountpercentage': v.additionaldiscountpercentage, 'receivable': v.receivable
                });
            });

            var dialog = ngDialog.open({
                template: 'AdditionalDiscountDialog',
                data: [$scope, list],
                width: '800px',
                controller: ['$scope', '$timeout', function ($scope, $timeout) {
                    $scope.PopupList = $scope.ngDialogData[1];
                    $scope.Approve = function () {
                        $scope.closeThisDialog($scope.PopupList);
                    };
                    $scope.DiscountChanged = function (type, item, newvalue, oldvalue) {
                        if (type == '%') {
                            item.additionaldiscount = (item.core.salerate * newvalue / 100).round(2);
                            item.additionaldiscountpercentage = newvalue;
                        }
                        else {
                            item.additionaldiscount = newvalue;
                            item.additionaldiscountpercentage = ((newvalue / item.core.salerate) * 100).round(2);
                        }
                        item.receivable = (item.core.receivable + (item.core.additionaldiscount)) - (item.additionaldiscount*item.core.qty);
                    };
                }],
                className: 'ngdialog-theme-plain'
            });
            dialog.closePromise.then(function (data) {
                {
                    if (typeof (data.value) != 'undefined' && data.value != null && data.value != '$escape' && data.value != '$document') {
                        $scope.CurrentBill.tax = 0;
                        $scope.CurrentBill.discount = 0;
                        $scope.CurrentBill.receivable = 0;
                        $scope.CurrentBill.rounding = 0;
                        $scope.CurrentBill.total = 0;
                        $scope.CurrentBill.additionaldiscount = 0;
                        $.each(data.value, function (k, v) {
                            v.core.additionaldiscount = $scope.GetNumber(v.additionaldiscount);
                            v.core.additionaldiscountpercentage = $scope.GetNumber(v.additionaldiscountpercentage);

                            if (typeof (v.core.barcodeDetail) == 'undefined') {
                                v.core.barcodeDetail = { 'taxid': v.core.taxid };

                                if (v.core.taxid > 0) {
                                    v.core.taxInfo = $scope.SalesTax.find(function (arritem) {
                                        return arritem.id == v.core.taxid;
                                    });

                                    if (v.core.taxInfo != null)
                                        v.core.taxInfo.taxsplit.findTax = $scope.findTax;
                                }
                                v.core.barcodeDetail.discount = v.core.discount;
                                v.core.barcodeDetail.additionaldiscount = 0;
                            }
                            $scope.UpdateTax(v.core);
                            $scope.UpdateItemInfo(v.core, false);
                        });
                        $scope.AllowAdditionalDiscount = ($scope.CurrentBill.additionaldiscount > 0);
                    }
                }
            });
        }

        //template: 'SettlementDialog',

        $scope.ShowSettlement = function (bill) {
            var dialog = ngDialog.open({
                template: 'SettlementDialog',
                data: [$scope, bill],
                width: '650px',
                controller: 'SettlementsController',
                className: 'ngdialog-theme-default'
            });
            dialog.closePromise.then(function (data) {
                {
                    $scope.setFocusautocoding();
                }
            });
        }

        $scope.ShowDirectSettlement = function (bill) {
            var dialog = ngDialog.open({
                template: 'clientside/views/Settlements_dialog.html',
                data: [$scope, bill],
                width: '1000px',
                controller: 'SettlementsController',
                className: 'ngdialog-theme-default'
            });
            dialog.closePromise.then(function (data) {
                {
                    $scope.setFocusautocoding();
                }
            });
        }

        /*
        update billitems set tax = round(billitems.receivable::numeric-(billitems.rate::numeric*billitems.qty::numeric),2) 
        from bill
        where bill.id = billitems.billid and  bill.billdate>='2017-10-01' and abs((round(billitems.receivable::numeric-(billitems.rate::numeric*billitems.qty::numeric),2))-billitems.tax::numeric)>0.1

        

update billitems set rate=round(((billitems.price::numeric-billitems.additionaldiscount::numeric)/(100+taxpercentage::numeric)*100),2),
        itemtax = price::numeric- round(((billitems.price::numeric-billitems.additionaldiscount::numeric)/(100+taxpercentage::numeric)*100),2),
        tax = round((((billitems.price::numeric-billitems.additionaldiscount::numeric)/(100+taxpercentage::numeric)*taxpercentage::numeric) *billitems.qty::numeric),2) 
        from bill
        where bill.id = billitems.billid and abs(round((rate::numeric*billitems.qty::numeric*billitems.taxpercentage/100)::numeric,2) - billitems.tax::numeric)>0.1



         select bill.billno,bill.id,billitems.tax,billitems.receivable-billitems.tax as avalue,billitems.qty,billitems.receivable,taxpercentage,rate,billitems.price,billitems.discount,billitems.additionaldiscount,
 round((rate::numeric*billitems.qty::numeric*billitems.taxpercentage/100)::numeric,2)
        from billitems
        inner join bill on bill.id = billitems.billid
        where abs(round((rate::numeric*billitems.qty::numeric*billitems.taxpercentage/100)::numeric,2) - billitems.tax::numeric)>0.1
limit 100


        */

        $timeout(function () {

            //var availableTags = [
			//"ActionScript",
			//"AppleScript",
			//"Asp",
			//"BASIC",
			//"C",
			//"C++",
			//"Clojure",
			//"COBOL",
			//"ColdFusion",
			//"Erlang",
			//"Fortran",
			//"Groovy",
			//"Haskell",
			//"Java",
			//"JavaScript",
			//"Lisp",
			//"Perl",
			//"PHP",
			//"Python",
			//"Ruby",
			//"Scala",
			//"Scheme"
            //];

            //$("#gBox").autocomplete({
            //    source: availableTags,
            //    delay: 500
            //});

            $scope.setFocusautocoding();
            if (typeof ($rootScope.LocationInfo.billconfig) != 'undefined' &&
                $rootScope.LocationInfo.billconfig != null) {
                $scope.billprefix = $rootScope.LocationInfo.billconfig.prefix;
                $scope.bulkbillprefix = $rootScope.LocationInfo.bulkbillconfig.prefix;
                if (typeof ($scope.billprefix) == 'undefined' || $scope.billprefix == '')
                    $scope.billprefix = null;
                if (typeof ($scope.bulkbillprefix) == 'undefined' || $scope.bulkbillprefix == '')
                    $scope.bulkbillprefix = null;
            }
            $rootScope.slideToggeler.Toggle(true);
        }, 10);

    }]);