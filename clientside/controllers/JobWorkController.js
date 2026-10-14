var app = angular.module('InfinityLab');

app.controller('JobWorkController', ['UserDetails', '$scope', '$http', '$location', '$rootScope', 'restService', '$timeout', 'Excel', 'ngDialog', '$stateParams',
    function (UserDetails, $scope, $http, $location, $rootScope, restService, $timeout, Excel, ngDialog, $stateParams) {

	$scope.system_jobworkgstforall = false;
        $scope.system_b2bexclusivetax = false;
        $scope.ShowImage = false;
        $scope.Navigation = $rootScope.GetNavigation();
        Number.prototype.round = function (places) {
            return +(Math.round(this + "e+" + places) + "e-" + places);
        }

        $scope.GetNumber = function (value) {
            value = parseFloat(value);
            if (typeof (value) == 'undefined' || value == null || isNaN(value))
                return 0;
            return value;
        };

        $scope.ProductList = restService.Get('ReferencelistService/PRODUCTS', true).then(function (results) {
            $scope.ProductList = results;
        });

        $scope.Company = restService.Get('ReferencelistService/BUYINGCOMPANY', true).then(function (results) {
            $scope.Company = results;
        });

        $scope.Permission = UserDetails.Info;
        $scope.SearchRef = restService.SearchReference;

        $scope.Agents = restService.Get('ReferencelistService/AGENT', true).then(function (results) {
            $scope.Agents = results;
        });

        $scope.Cities = restService.Get('ReferencelistService/CITY', true).then(function (results) {
            $scope.Cities = results;
        });

        $scope.JobChargeList = restService.Get('ReferencelistService/FULL-JOBCHARGE', true).then(function (results) {
            $scope.JobChargeList = results;
        });
        $scope.OrderList = {};
        $scope.StockResult = [];

        $scope.Entity = {
            "supplierid": 0,
            "tax": 0,
            "amount": 0,
            "taxable": 0,
            "qty": 0,
            "id": 0,
            billmode: "",
            materialcost: "",
            materialsalevalue: "",
            settled: false,
            version: 1,
            "locationid": 0,
            "suppliercompanyid": 0,
            "companyid": 0,
            "barcode": null,
            "stockid": 0,
            "Jobworkbillitems": [],
            "taxid": 0,
            "rate": 0.0,
            "noofpieces": 0,
            "paidamount": 0,
            "invoiceno": null,
            "entrydate": new Date(),
            "invoicedate": new Date(),
            "lrinvoiceid": 0,
            "isactive": true,
            "createdby": 0,
            "modifiedby": 0,
            "version": 0,
            "createdon": "0001-01-01T00:00:00",
            "modifiedon": "0001-01-01T00:00:00",
            taxid: 0,
            barcode: "",
            jobchargeid: ""

        };

        $scope.New = function () {
            $scope.LoadReturnList = [];
            $scope.Jobwork = (JSON.parse(JSON.stringify($scope.Entity)));
            $scope.Jobwork.invoicedate = new Date();
            $scope.Jobwork.Lrinvoicereturnitems = [];
            //$scope.CalculateTotal();
            $scope.Jobwork.Jobworkbillitems = [];
        }
        $scope.New();

        $scope.Suppliers = restService.Get('ReferencelistService/SUPPLIER', true).then(function (results) {
            $scope.Suppliers = results;
        });

        $scope.TaxList = restService.Get('ReferencelistService/SALESTAX', true).then(function (results) {
            $scope.TaxList = results;
            results.push({
                'id': -1, 'name': 'Not Eligible', 'type': 1, 'taxpercentage': 0
            });
        });

        $scope.Locations = restService.Get('ReferencelistService/RETAILLOCATION', true).then(function (results) {
            $scope.Locations = results;
        });


        $scope.Counters = restService.Get('ReferencelistService/COUNTER', true).then(function (results) {
            $scope.Counters = results;
        });

        $scope.Brand = restService.Get('ReferencelistService/BRAND', true).then(function (results) {
            $scope.Brand = results;
        });
        $scope.ErrorStyle = "success";

        $scope.ShowSearch = false;
        $scope.Debitnote = false;

        $scope.SearchResult = [];
        $scope.StockResult = [];

        $scope.SwitchPanel = function () {
            $scope.ShowSearch = !$scope.ShowSearch;
        }


        $scope.SearchRequest = {
            returndate: ""
        };

        $scope.QtyChaged = function () {
            var oqty = 0;
            if ($scope.OrderList && $scope.OrderList.oqty) {
                oqty = parseFloat($scope.OrderList.oqty);
            }
            if (isNaN(oqty)) oqty = 0;
            if (typeof ($scope.Jobwork.jobcharge) == 'undefined') $scope.Jobwork.jobcharge = 0;
            if (isNaN($scope.Jobwork.jobcharge)) $scope.Jobwork.jobcharge = 0;

            $scope.OrderList.chargerate = $scope.Jobwork.jobcharge;
            $scope.OrderList.chargegross = oqty * $scope.Jobwork.jobcharge;
            $scope.OrderList.chargetaxid = $scope.Jobwork.taxid;
        }

        $scope.TaxChanged = function () {
            if ($scope.system_jobworkgstforall ||
                $scope.Jobwork.limit_billvaluetaxable == undefined || $scope.Jobwork.limit_billvaluetaxable) {
                var tax = $scope.TaxList.find(function (item) {
                    return item.id == $scope.Jobwork.taxid;
                });
                if (tax && tax != null) {
                    $scope.OrderList.chargetaxpercentage = tax.taxpercentage;
                    if ($scope.Jobwork.chargetaxpercentage == undefined ||
                        $scope.Jobwork.chargetaxpercentage != tax.taxpercentage) {
                        $scope.Jobwork.chargetaxpercentage = tax.taxpercentage;
                        $scope.ChangeTaxValue();
                    }
                }
                $scope.QtyChaged();
            }
            else {
                $scope.OrderList.chargetaxpercentage = 0;
            }
        }

        $scope.ChangeTaxValue = function () {
            if (typeof ($scope.Jobwork.Jobworkbillitems) == 'undefined' || $scope.Jobwork.Jobworkbillitems == null)
                $scope.Jobwork.Jobworkbillitems = [];

            $.each($scope.Jobwork.Jobworkbillitems, function (k, v) {
                v.chargetaxid = $scope.Jobwork.taxid;
                v.chargetaxpercentage = $scope.Jobwork.chargetaxpercentage;
                v.chargetax = (v.chargegross * v.chargetaxpercentage / 100).round(2);
                $scope.OrderList.chargetotal = ($rootScope.GetNumber($scope.OrderList.chargegross) + $rootScope.GetNumber($scope.OrderList.chargetax)).round(0);
            })

            $scope.CalculateTotal();
        }

        $scope.ChargeChanged = function () {
            $scope.Jobwork.jobcharge = 0;
            $scope.Jobwork.taxid = 0;

            var jobcharge = $scope.JobChargeList.find(function (item) {
                return item.id == $scope.Jobwork.jobchargeid;
            });
            if (jobcharge && jobcharge != null && jobcharge.additionalinfo &&
                jobcharge.additionalinfo != null && jobcharge.additionalinfo.mfgconfig &&
                jobcharge.additionalinfo.mfgconfig != null) {
                $scope.Jobwork.jobcharge = jobcharge.additionalinfo.mfgconfig.charge;
                $scope.Jobwork.taxid = jobcharge.additionalinfo.mfgconfig.taxid;
            }
            $scope.TaxChanged();
        }

        $scope.FilterSupplierCompany = function () {

            if ($scope.Jobwork.Jobworkbillitems &&
                $scope.Jobwork.Jobworkbillitems.length > 0) {
                $scope.Jobwork.Jobworkbillitems = [];
            }

            $scope.Jobwork.limit_billvalue = 0;
            $scope.Jobwork.limit_billvaluetaxable = false;
            $scope.Jobwork.taxenabled = false;
            $scope.TaxStyle = {};

            var suppliercompany = $scope.Suppliers.find(function (item) {
                return item.id == $scope.Jobwork.supplierid;
            });

            if (typeof (suppliercompany) != 'undefined' && suppliercompany != null) {
                $scope.Jobwork.current_billvalue = 0
                $scope.Jobwork.suppliercompanyid = suppliercompany.suppliercompanyid;
                $scope.Jobwork.isinterstatesale = suppliercompany.isinterstatesale;
                $scope.Jobwork.limit_billvalue = suppliercompany.limit_billvalue;
                $scope.Jobwork.limit_billvaluetaxable = suppliercompany.limit_billvaluetaxable;
                if ($scope.Jobwork.limit_billvaluetaxable || $scope.Jobwork.limit_billvaluetaxable == undefined)
                    $scope.TaxStyle = { "background-color": "lightgrey" };

                if (!$scope.Jobwork.limit_billvaluetaxable && $scope.Jobwork.limit_billvalue > 0) {
                    request = {
                        "item": $scope.Jobwork,
                        "securitytoken": "String content"
                    }
                    var srequest = JSON.stringify(request);

                    $http({
                        method: 'POST',
                        url: "JobworkbillService/SearchBillValue",
                        data: srequest
                    }).success(function (result) {
                        $scope.Jobwork.current_billvalue = result;
                    });
                }
            }
        };

        $scope.LocationChanged = function () {
            var location = $scope.Locations.find(function (item) {
                return item.id == $scope.Jobwork.locationid;
            });
            if (location && location != null && typeof (location.productgroup) != 'undefined') {
                $scope.Jobwork.companyid = location.productgroup;
            }
        }

        $scope.exporttoExcel = function (tableid, title) {
            $scope.exportHref = $scope.igrid.ExportExcel($scope.igrid.grid, title);
            //$timeout(function () {
            //    var link = document.createElement('a');
            //    link.download = title + ".xls";
            //    link.href = $scope.exportHref;
            //    document.body.appendChild(link);
            //    link.click();
            //    link.remove();
            //}, 100);
            return false;
        }

        $scope.ErrorMessage = "";


        $scope.SearchCondition = {
            barcode: "",
        }


        $scope.OrderList = {
            productname: "",
            barcode: "",
            design: "",
            rate: "",
            stockqty: "",
            qty: "",
            total: ""
        }

        $scope.TryAddItem = function () {

            $scope.OrderList.chargegross = parseFloat($scope.OrderList.chargegross);
            $scope.OrderList.oqty = parseFloat($scope.OrderList.oqty);

            if (isNaN($scope.OrderList.chargegross)) $scope.OrderList.chargegross = 0;
            if (isNaN($scope.OrderList.oqty)) $scope.OrderList.oqty = 0;

            if (typeof ($scope.OrderList.outproductid) == 'undefined' ||
                $scope.OrderList.outproductid <= 0) {
                $rootScope.Toaster.Info("Please select receiving product");
                return;
            }
            if (typeof ($scope.OrderList.productid) == 'undefined' ||
                $scope.OrderList.productid <= 0) {
                $rootScope.Toaster.Info("Please select from product");
                return;
            }
            if (typeof ($scope.OrderList.stockid) == 'undefined' ||
                $scope.OrderList.stockid <=0) {
                $rootScope.Toaster.Info("Please select barcode");
                return;
            }
            if (typeof ($scope.OrderList.oqty) == 'undefined' ||
                $scope.OrderList.oqty <= 0) {
                $rootScope.Toaster.Info("Please enter out qty");
                return;
            }
            if (typeof ($scope.OrderList.iqty) == 'undefined' ||
                $scope.OrderList.iqty <= 0) {
                $rootScope.Toaster.Info("Please enter in qty");
                return;
            }
            if ($scope.OrderList.chargegross == 0) {
                $rootScope.Toaster.Info("Please enter the total charge of jobwork for this item");
                return;
            }

            if (typeof ($scope.Jobwork.Jobworkbillitems) == 'undefined' || $scope.Jobwork.Jobworkbillitems == null)
                $scope.Jobwork.Jobworkbillitems = [];

            
            $scope.OrderList.chargerate = ($scope.OrderList.chargegross / $scope.OrderList.oqty).round(2);
            $scope.OrderList.chargetax = ($scope.OrderList.chargegross * $scope.OrderList.chargetaxpercentage / 100).round(2);
            if (isNaN($scope.OrderList.chargetax)) $scope.OrderList.chargetax = 0;

            $scope.OrderList.chargetotal = ($rootScope.GetNumber($scope.OrderList.chargegross) + $rootScope.GetNumber($scope.OrderList.chargetax)).round(0);

            $scope.Jobwork.Jobworkbillitems.push(
                {
                    'productname': $scope.OrderList.productname,
                    'barcode': $scope.OrderList.barcode,
                    'designid': $scope.OrderList.designid,
                    'materialsalevalue': $scope.OrderList.materialsalevalue,
                    'materialcost': $scope.OrderList.materialcost,
                    'outproductid': $scope.OrderList.outproductid,
                    'productid': $scope.OrderList.productid,
                    'invoiceitemid': $scope.OrderList.invoiceitemid,
                    'hsncode': $scope.OrderList.hsncode,
                    'salablegoodsid': $scope.OrderList.salablegoodsid,
                    'stockid': $scope.OrderList.stockid,
                    'iqty': $scope.OrderList.iqty,
                    'oqty': $scope.OrderList.oqty,
                    'chargetaxid': $scope.OrderList.chargetaxid,
                    'chargeid': $scope.Jobwork.jobchargeid,
                    'chargegross': $scope.OrderList.chargegross,
                    'chargetax': $scope.OrderList.chargetax,
                    'chargetotal': $scope.OrderList.chargetotal,
                    'chargetaxpercentage': $scope.OrderList.chargetaxpercentage,
                    'chargerate': $scope.OrderList.chargerate,
                });

            $scope.Jobwork.barcode = '';
            $scope.OrderList.stockqty = '';
            $scope.OrderList.outproductid = null;
            $scope.OrderList.productname = '';
            $scope.OrderList.designid = '';
            $scope.OrderList.materialsalevalue = '';
            $scope.OrderList.materialcost = '';
            $scope.OrderList.iqty = '';
            $scope.OrderList.oqty = '';
            $scope.OrderList.chargegross = '';
            $scope.OrderList.chargetax = '';
            $scope.OrderList.chargetotal = '';
            $scope.OrderList.imageid = '';
            $scope.CalculateTotal();
            $scope.setFocusjobcharge();
            $scope.ShowImage = false;
            $scope.currentimageid = null;
        };

        $scope.SearchStock = function () {
            $scope.StockList = [];
            $scope.saleqty = 0;
            $scope.amount = 0;
            if (($scope.Jobwork.locationid == 0 || $scope.Jobwork.locationid == 'undefined')) {
                $rootScope.Toaster.Info("Please Select The Location..");
                return;
            }
            request = {
                "item": $scope.Jobwork,
                "securitytoken": "String content"
            }
            var srequest = JSON.stringify(request);

            $http({
                method: 'POST',
                url: "JobworkbillService/SearchStock",
                data: srequest
            }).success(function (result) {
                if (result == null || result.length == 0) {
                    $rootScope.Toaster.Info("No Results Found.");
                    return;
                }
                $scope.StockResult = result;
                $scope.OrderList.productname = $scope.StockResult[0].productname;
                $scope.OrderList.materialsalevalue = $scope.StockResult[0].price;
                $scope.OrderList.stockqty = $scope.StockResult[0].qty;
                $scope.OrderList.barcode = $scope.StockResult[0].barcode;
                $scope.OrderList.designid = $scope.StockResult[0].design;
                if($scope.system_b2bexclusivetax)
                    $scope.OrderList.materialcost = $scope.StockResult[0].buyingprice;
                else
                    $scope.OrderList.materialcost = $scope.StockResult[0].ibuyingprice;


                $scope.OrderList.productid = $scope.StockResult[0].productid;
                $scope.OrderList.invoiceitemid = $scope.StockResult[0].invoiceitemid;
                $scope.OrderList.hsncode = $scope.StockResult[0].hsncode;
                $scope.OrderList.salablegoodsid = $scope.StockResult[0].salablegoodsid;
                $scope.OrderList.stockid = $scope.StockResult[0].stockid;
                $scope.OrderList.outproductid = $scope.StockResult[0].productid;
                $scope.OrderList.imageid = $scope.StockResult[0].imageid;
                $scope.ShowImage = false;
                $scope.currentimageid = null;
                if ($scope.OrderList.imageid && $scope.OrderList.imageid != '') {
                    $scope.ShowImage = true;
                    $scope.currentimageid = $scope.StockResult[0].imageid;
                }
                $scope['setFocusoutproductid']();
            }).error(function (result) {
            });
        }

        $scope.ManageItem = function (type, item) {
            if (type == 'Edit') {
                $scope.OrderList = item;
                $scope.Jobwork.barcode = item.barcode;
                $scope.OrderList.productname = $scope.SearchRef(item.productid, $scope.ProductList);
                $scope.Jobwork.Jobworkbillitems.splice($scope.Jobwork.Jobworkbillitems.indexOf(item), 1);
                $scop.ChargeChanged();
            }
            else {
                $scope.Jobwork.Jobworkbillitems.splice($scope.Jobwork.Jobworkbillitems.indexOf(item), 1);
            }
            $scope.CalculateTotal();
        };


        $scope.CalculateTotal = function () {
            $scope.Jobwork.taxamount = 0;
            $scope.Jobwork.discount = 0;
            $scope.Jobwork.grossamount = 0;
            $scope.Jobwork.totalamount = 0;
            $scope.Jobwork.qty = 0;
            $scope.Jobwork.oqty = 0;
            $scope.Jobwork.materialcost = 0;
            $scope.Jobwork.materialsalevalue = 0;
            $.each($scope.Jobwork.Jobworkbillitems, function (k, v) {
                $scope.Jobwork.grossamount = $scope.GetNumber(v.chargegross) + $scope.Jobwork.grossamount;
                $scope.Jobwork.taxamount = $scope.GetNumber(v.chargetax) + $scope.Jobwork.taxamount;
                $scope.Jobwork.totalamount = $scope.GetNumber(v.chargetotal) + $scope.Jobwork.totalamount;
                $scope.Jobwork.qty = $scope.GetNumber(v.iqty) + $scope.Jobwork.qty;
                $scope.Jobwork.oqty = $scope.GetNumber(v.oqty) + $scope.Jobwork.oqty;
                $scope.Jobwork.materialcost += $scope.GetNumber(v.iqty) * v.materialcost;
                $scope.Jobwork.materialsalevalue += $scope.GetNumber(v.iqty) * v.materialsalevalue;
                v.sno = k + 1;
            });
        }


        $scope.Save = function () {



            if ($scope.Jobwork.companyid == null || $scope.Jobwork.companyid == 0) {
                $rootScope.Toaster.Info("Please Select Company.!");
                return;
            }

            if ($scope.Jobwork.supplierid == null || $scope.Jobwork.supplierid == 0) {
                $scope['setFocussupplierid']();
                $rootScope.Toaster.Info("Please Select Supplier.!");
                return;
            }

            if ($scope.Jobwork.limit_billvalue > 0 && 
                $scope.Jobwork.limit_billvalue< $scope.Jobwork.current_billvalue + $scope.Jobwork.grossamount) {
                $rootScope.Toaster.Error("Non taxable bill value crossed the limit");
                return;
            }

            var request = {
                "Item": (JSON.parse(JSON.stringify($scope.Jobwork))),
                "securitytoken": "String content"
            }
            var srequest = JSON.stringify(request);

            $http({
                method: 'POST',
                url: "JobworkbillService/Save",
                data: srequest
            }).success(function (result) {
                if (result != null && typeof (result.length) == 'undefined' && result.id > 0) {
                    $rootScope.Toaster.Info("Job Work Successfully Save with Invoice no : " + result.invoiceno);
                    $scope.Jobwork = (JSON.parse(JSON.stringify($scope.Entity)));
                }
                else {
                    $rootScope.Toaster.Error(result.errormessage);
                }
            }).error(function (result) {
            });
        }


        $scope.ManageSearchItem = function (action, item) {

            if (action == "Print") {
                $scope.Print(item.id);
                return;
            }
            if (action == "Delete") {
                $.confirm({
                    title: 'Delete',
                    content: 'Do you want to delete ' + item.invoiceno + "?",
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
                                request = {
                                    "Item": item,
                                    "SecurityToken": "String content"
                                }
                                var srequest = JSON.stringify(request);
                                $http({
                                    method: 'POST',
                                    url: "JobworkbillService/Delete",
                                    data: srequest
                                }).success(function (result) {

                                    if (result) {
                                        $scope.ErrorMessage = "Successfully deleted";
                                        $scope.SearchResult.splice($scope.SearchResult.indexOf(item), 1);
                                        $scope.igrid.UpdateData($scope.SearchResult);
                                    }
                                }).error(function (result) {

                                });
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
            }
            else if (action == "Edit") {
                $scope.ShowSearch = false;

                request = {
                    "Item": { 'id': item.id },
                    "securitytoken": "String content"
                }

                var srequest = JSON.stringify(request);
                $http({
                    method: 'POST',
                    url: "JobworkbillService/Search",
                    data: srequest
                }).success(function (result) {
                    $scope.Jobwork = result[0];
                    $scope.Navigation.SetMode('EDIT');
                }).error(function (result) {
                });
            }
        }

        $scope.Print = function (id) {
            $rootScope.Download('JobworkbillService/PrintPDF/' + $scope.UserInfo.OrganizationID + '/' + (typeof (id) == 'undefined' ? 0 : id), 'jobworkorder.pdf', 'application/pdf');
            return;

            $timeout(function () {
                var link = document.createElement('a');
                link.download = "jobworkorder.pdf";
                link.href = 'JobworkbillService/PrintPDF/' + $scope.UserInfo.OrganizationID + '/' + (typeof (id) == 'undefined' ? 0 : id);
                document.body.appendChild(link);
                link.click();
                $timeout(function () {
                    link.remove();
                }, 100);
            }, 10);
        }
        $scope.CreateJobGRN = function (id) {
                $location.url('/JobWorkGRN?ids=' + id);
        }

        $scope.Search = function () {
            $scope.ShowSearch = true;

            request = {
                "Item": $scope.SearchCondition,
                "securitytoken": "String content"
            }

            var srequest = JSON.stringify(request);
            $http({
                method: 'POST',
                url: "JobworkbillService/Search",
                data: srequest
            }).success(function (result) {
                $scope.SearchResult = result;
                $scope.JobWorkConfig();
                if (result == 'undefined' || result == null || result.length == 0)
                    $rootScope.Toaster.Info("No Results Found.");
            }).error(function (result) {
            });

        }

        $scope.JobWorkConfig = function () {
            if ($scope.igrid != null)
                $scope.igrid.destroy();

            var igrid = new InfyGrid('gridContainer', '450px', '100%', true, false, true, 20);
            igrid.options.showHeaderRow = true;
            igrid.EnableSerial($scope.SearchResult);
            igrid.AddSimpleColumn('invoiceno', 'JobWork Order No', 100);
            igrid.AddFormatedColumn('entrydate', 'Order Date', 85, 'dd-MM-yyyy');
            igrid.AddFormatedColumn('invoicedate', 'Inv. Date', 85, 'dd-MM-yyyy');
            igrid.AddFormatedColumn('expecteddate', 'Due Date', 85, 'dd-MM-yyyy');
            igrid.AddReferenceColumn('supplierid', 'Job Worker', 170, $scope.Suppliers);
            igrid.AddReferenceColumn('locationid', 'Stock Location', 100, $scope.Locations);
            igrid.AddFormatedColumn('qty', 'Out Qty', 70, 'N2', 2).total = true;
            igrid.AddFormatedColumn('oqty', 'Exp Qty', 70, 'N2', 2).total = true;
            igrid.AddFormatedColumn('receivedqty', 'Received Qty', 80, 'N2', 2).total = true;
            igrid.AddFormatedColumn('totalamount', 'Job Work Amount', 100, 'N2', 2).total = true;
            igrid.AddSimpleColumn('statustext', 'Status', 80);

            var commands = '';
            if ($rootScope.UserInfo.p_STOREJOBWORK.modify)
                commands += "<input type='button' class='btn btn-primary btn-xs' value='Edit'/> &nbsp;"
            if ($rootScope.UserInfo.p_STOREJOBWORK.delete)
                commands += "<input type='button' class='btn btn-danger btn-xs' value='Delete'/> &nbsp;"
            if ($rootScope.UserInfo.p_STOREJOBWORK.print)
                commands += "<input type='button' class='btn btn-info btn-xs' value='Print'/> &nbsp;"

            if (commands != '') {
                igrid.AddButtonColumn('action', 'Action', 160, commands, function (e, args) {
                    $scope.$apply(function () {
                        $scope.ManageSearchItem($(e.target).val(), args.item);
                    });
                    return true;
                });
            }

            igrid.options.autoEdit = true;
            igrid.EnableTotal();
            igrid.Initialize();
            $scope.igrid = igrid;
            $scope.igrid.UpdateData($scope.SearchResult);

        }

        $scope.ClearItems = function () {
            $scope.Jobwork.Jobworkbillitems = [];
            $scope.CalculateTotal();
        }

        $scope.Appsetting_Company = false;
        $scope.LoadApplicationSetting = function () {
            if ($rootScope.UserInfo && $rootScope.UserInfo != null &&
                $rootScope.UserInfo.appsettings && $rootScope.UserInfo.appsettings!=null) {
                $scope.Appsetting_Company = true;
                $scope.system_b2bexclusivetax = false || ($rootScope.UserInfo.appsettings && $rootScope.UserInfo.appsettings.system_b2bexclusivetax);
                $scope.system_jobworkgstforall = false || ($rootScope.UserInfo.appsettings && $rootScope.UserInfo.appsettings.system_jobworkgstforall);
                if (typeof ($location.search().id) != 'undefined' &&
                    $location.search().id.length > 0) {
                    $scope.ManageSearchItem('Edit', { 'id': $location.search().id });
                }
            }
            else {
                $timeout(function () {
                    $scope.LoadApplicationSetting();
                }, 50);
            }
        }

        $timeout(function () {
            $scope.LoadApplicationSetting();
        }, 10);

    }]);

