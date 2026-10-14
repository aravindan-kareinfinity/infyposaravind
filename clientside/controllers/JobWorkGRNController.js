var app = angular.module('InfinityLab');
app.controller('JobWorkGRNController', ['UserDetails', '$scope', '$http', '$location', '$rootScope', 'restService', '$timeout', 'Excel', 'ngDialog', '$stateParams',
    function (UserDetails, $scope, $http, $location, $rootScope, restService, $timeout, Excel, ngDialog, $stateParams) {

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


        $scope.Company = restService.Get('ReferencelistService/BUYINGCOMPANY', true).then(function (results) {
            $scope.Company = results;
        });
        $scope.Permission = UserDetails.Info;
        $scope.SearchRef = restService.SearchReference;

        $scope.SleeveList = restService.Get('ReferencelistService/SLEEVE', true).then(function (results) {
            $scope.SleeveList = results;
        });

        $scope.ProductList = restService.Get('ReferencelistService/PRODUCTS', true).then(function (results) {
            $scope.ProductList = results;
        });
        $scope.BrandList = restService.Get('ReferencelistService/BRAND', true).then(function (results) {
            $scope.BrandList = results;
        });
        $scope.PatternList = restService.Get('ReferencelistService/PATTERN', true).then(function (results) {
            $scope.PatternList = results;
        });
        $scope.StyleList = restService.Get('ReferencelistService/STYLE', true).then(function (results) {
            $scope.StyleList = results;
        });
        $scope.SizeList = restService.Get('ReferencelistService/SIZE', true).then(function (results) {
            $scope.SizeList = results;
            results.push({ 'id': -2, 'name': 'Jump Size' });
        });
        $scope.ColourList = restService.Get('ReferencelistService/COLOUR', true).then(function (results) {
            $scope.ColourList = results;
        });

        $scope.TypeList = restService.Get('ReferencelistService/TYPE', true).then(function (results) {
            $scope.TypeList = results;
        });

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
            "status": 1,
            "amount": 0,
            "taxable": 0,
            "qty": 0,
            "id": 0,
            billmode: "",
            materialcost: "",
            materialsalevalue: "",
            settled: false,
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
        }
        $scope.New();

        $scope.Suppliers = restService.Get('ReferencelistService/SUPPLIER', true).then(function (results) {
            $scope.Suppliers = results;
        });

        $scope.TaxList = restService.Get('ReferencelistService/SALESTAX', true).then(function (results) {
            $scope.TaxList = results;
        });

        $scope.Status = [{ 'id': 1, 'name': 'Progress' }, { 'id': 2, 'name': 'Completed' }]
        $scope.Locations = restService.Get('ReferencelistService/RETAILLOCATION', true).then(function (results) {
            $scope.Locations = results;
            results.push({ 'id': 0, 'name': 'Warehouse', 'parentgroup': 0 });
        });

        $scope.PurchaseTax = restService.Get('ReferencelistService/PURCHASETAX', true).then(function (results) {
            $scope.PurchaseTax = results;
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

        $scope.FindBarcode = function () {
            if ($scope.OrderList.barcode == '0') {
                $scope.Save();
                return;
            }
            var item = $scope.PendingResult.find(function (item) {
                return item.barcode == $scope.OrderList.barcode;
            });

            var jobitem = $scope.Jobwork.Jobworkbillitems.find(function (item) {
                return item.barcode == $scope.OrderList.barcode;
            });

            if (typeof (jobitem) != 'undefined' && jobitem != null) {
                $rootScope.Toaster.Error("Already added.");
                return;
            }

            if (item && item != null) {
                $scope.ShowItem(item);
            }
        }

        $scope.SearchPending = function (jobworkbillgrnid, jobworkids) {
            request = {
                "item": {
                    'supplierid': $scope.Jobwork.supplierid,
                    'locationid': $scope.Jobwork.locationid,
                    'companyid': $scope.Jobwork.companyid,
                    'invoicedate': $scope.Jobwork.invoicedate,
                    'invoiceno': $scope.Jobwork.invoiceno,
                    'jobworkbillgrnid': jobworkbillgrnid,
                    'jobworkids': jobworkids,
                    'includeitemdetails': true
                },
                "securitytoken": "String content"
            }
            var srequest = JSON.stringify(request);

            $http({
                method: 'POST',
                url: "JobworkbillitemsService/SearchPending",
                data: srequest
            }).success(function (result) {
                $scope.PendingResult = result;


                if ($scope.pgrid != null)
                    $scope.pgrid.destroy();

                var pgrid = new InfyGrid('gridPending', '450px', '100%', true, false, true, 220);
                pgrid.options.showHeaderRow = true;

                pgrid.AddSimpleColumn('invoiceno', 'Order No', 60);
                pgrid.AddSimpleColumn('barcode', 'Barcode', 60);
                pgrid.AddReferenceColumn('outproductid', 'Product', 100, $scope.ProductList);
                pgrid.AddFormatedColumn('entrydate', 'Entry Date', 60, 'dd-MM-yyyy');
                pgrid.AddFormatedColumn('oqty', 'Issued', 40, 'N2', 2).total = true;
                pgrid.AddFormatedColumn('rqty', 'Received', 40, 'N2', 2).total = true;

                var commands = "";
                commands += "<input type='button' class='btn btn-primary btn-xs' value='I'/> &nbsp;"
                pgrid.AddButtonColumn('action', '&nbsp;', 30, commands, function (e, args) {
                    $scope.$apply(function () {
                        $scope.ValidateAndShowItem(args.item);
                    });
                });

                pgrid.Initialize();
                $scope.pgrid = pgrid;
                $scope.pgrid.UpdateData($scope.PendingResult, '_id');

                if (jobworkids && jobworkids.length > 0 && $scope.PendingResult.length > 0) {
                    $scope.Jobwork.locationid = $scope.PendingResult[0].locationid;
                    $scope.Jobwork.companyid = $scope.PendingResult[0].companyid;
                    $scope.Jobwork.supplierid = $scope.PendingResult[0].supplierid;
                    $scope.Jobwork.suppliercompanyid = $scope.PendingResult[0].suppliercompanyid;
                }
                $scope.setFocusfirst();
            });
        }


        $scope.ValidateAndShowItem = function (newitem) {
            if (!$scope.system_jobworksamebarcode) {
                $scope.ShowItem(newitem);
            }
            else {
                var jobitem = $scope.Jobwork.Jobworkbillitems.find(function (item) {
                    return item.barcode == newitem.barcode;
                });

                if (typeof (jobitem) != 'undefined' && jobitem != null) {
                    $rootScope.Toaster.Error("Already added.");
                    return;
                }
                else
                    $scope.ShowItem(newitem);
            }
        }

        $scope.ShowItem = function (item) {
            $scope.OrderList = (JSON.parse(JSON.stringify(item)));
            $scope.OrderList.id = 0;
            $scope.OrderList.productname = $scope.SearchRef(item.outproductid, $scope.ProductList);
            $scope.OrderList.jobworkbillitemsid = item.id;
            $scope.OrderList.productid = item.outproductid;
            $scope.OrderList.qty = item.oqty - item.rqty;
            $scope.OrderList.designid = item.designid;
            $scope.OrderList.chargegross = ($scope.OrderList.chargegross / item.oqty) * $scope.OrderList.qty;
            if (item.iteminfo != null) {
                $scope.OrderList.styleid = item.iteminfo.styleid;
                $scope.OrderList.sleeveid = item.iteminfo.sleeveid;
                $scope.OrderList.colourid = item.iteminfo.colourid;
                $scope.OrderList.brandid = item.iteminfo.brandid;
                $scope.OrderList.patternid = item.iteminfo.patternid;
                $scope.OrderList.materialid = item.iteminfo.materialid;
                $scope.OrderList.sizeid = item.iteminfo.sizeid;
                $scope.OrderList.typeid = item.iteminfo.typeid;
            }
            $scope.OrderList.chargetax = ($scope.GetNumber($scope.OrderList.chargegross) * $scope.GetNumber($scope.OrderList.chargetaxpercentage) / 100).round(2);
            if (isNaN($scope.OrderList.chargetax)) $scope.OrderList.chargetax = 0;
            
            $scope.OrderList.mcost = ((item.iqty / item.oqty) * item.materialcost).round(2);
            $scope.OrderList.mjtax = (item.chargerate * $scope.OrderList.chargetaxpercentage/100).round(2);
            $scope.OrderList.cost = ((item.iqty / item.oqty) * item.materialcost + item.chargerate + $scope.OrderList.mjtax).round(2);
            $scope.OrderList.finalprice = $scope.OrderList.cost;
            if (item.materialcost > 0 && item.materialsalevalue > 0 &&
                item.materialsalevalue > item.materialcost) {
                var marginpercentage = (item.materialsalevalue / item.materialcost * 100) - 100;
                $scope.OrderList.finalprice += (marginpercentage * $scope.OrderList.finalprice / 100);
                $scope.OrderList.finalprice = $scope.OrderList.finalprice.round(0);
                $scope.OrderList.dealerprice = $scope.OrderList.finalprice;
            }

            $scope.ShowImage = false;
            $scope.currentimageid = null;
            if ($scope.OrderList.imageid && $scope.OrderList.imageid != '') {
                $scope.ShowImage = true;
                $scope.currentimageid = item.imageid;
            }

            if ($scope.Jobwork.autocapture) {
                $scope.TryAddItem();
            }
            else {
                $scope.setFocusdesignid();
            }
        }

        $scope.QtyChaged = function () {
            var qty = 0;
            if ($scope.OrderList && $scope.OrderList.qty) {
                qty = parseFloat($scope.OrderList.qty);
            }
            if (isNaN(qty)) qty = 0;
            $scope.OrderList.chargegross = qty * $scope.OrderList.chargerate;
        }


        $scope.FilterSupplierCompany = function () {

            var suppliercompany = $scope.Suppliers.find(function (item) {
                return item.supplierid == $scope.Jobwork.id;
            });

            if (typeof (suppliercompany) != 'undefined' && suppliercompany != null)
                $scope.Jobwork.suppliercompanyid = suppliercompany.suppliercompanyid;

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
            if (typeof ($scope.pgrid) == 'undefined') return;
            $scope.exportHref = $scope.pgrid.ExportExcel($scope.pgrid.grid, 'Jobwork GRN List');
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
            if ($scope.SupportImageRecording) {
                $rootScope.CaptureImage('IMAGE', function (value) {
                    $scope.OrderList.imageid = value;
                    if ($scope.OrderList.sizeid != null && $scope.OrderList.sizeid != null &&
                        $scope.OrderList.sizeid == -2) {
                        $scope.CreateMultiItems($scope.OrderList, true, false, false, false);
                    }
                    else {
                        $scope.AddItem();
                    }
                });
            }
            else {
                if ($scope.OrderList.sizeid != null && $scope.OrderList.sizeid != null &&
                    $scope.OrderList.sizeid == -2) {
                    $scope.CreateMultiItems($scope.OrderList, true, false, false, false);
                }
                else {
                    $scope.AddItem();
                }
            }
        }

        $scope.AddItem = function () {
            $scope.ShowImage = false;
            $scope.currentimageid = null;
            $scope.OrderList.qty = parseFloat($scope.OrderList.qty);
            $scope.OrderList.chargegross = parseFloat($scope.OrderList.chargegross);
            
            if (isNaN($scope.OrderList.qty)) {
                $scope.OrderList.qty = 0;
                return;
            }
            if (isNaN($scope.OrderList.chargegross)) {
                $scope.OrderList.chargegross = 0;
                return;
            }
            if ($scope.OrderList.jobworkbillitemsid == 0) {
                return;
            }
            if (typeof ($scope.Jobwork.Jobworkbillitems) == 'undefined' || $scope.Jobwork.Jobworkbillitems == null)
                $scope.Jobwork.Jobworkbillitems = [];
            var available = $scope.PendingResult.find(function (item) {
                return item.id == $scope.OrderList.jobworkbillitemsid;
            });
            var availableqty = available.oqty - available.rqty;
            var consumedqty = $scope.OrderList.qty;

            $.each($scope.Jobwork.Jobworkbillitems, function (item) {
                if (item.jobworkbillitemsid == $scope.OrderList.jobworkbillitemsid)
                    consumedqty += parseFloat(item.qty);
            });

            if (isNaN(consumedqty) || consumedqty > availableqty) {
                $rootScope.Toaster.Info("Available qty - " + (availableqty - (consumedqty - $scope.OrderList.qty)));
            }

            $scope.OrderList.chargerate = ($scope.OrderList.chargegross / $scope.OrderList.qty).round(2);
            $scope.OrderList.chargetax = ($scope.GetNumber($scope.OrderList.chargegross) * $scope.GetNumber($scope.OrderList.chargetaxpercentage) / 100).round(2);
            if (isNaN($scope.OrderList.chargetax)) $scope.OrderList.chargetax = 0;
            $scope.OrderList.chargetotal = ($scope.GetNumber($scope.OrderList.chargegross) + $scope.GetNumber($scope.OrderList.chargetax)).round(0);
            var attributes = [];
            var sizedisplay = $scope.SearchRef($scope.OrderList.sizeid, $scope.SizeList);
            if (sizedisplay && sizedisplay != null && sizedisplay != '') attributes.push(sizedisplay);
            var styledisplay = $scope.SearchRef($scope.OrderList.styleid, $scope.StyleList);
            if (styledisplay && styledisplay != null && styledisplay != '') attributes.push(styledisplay);
            var branddisplay = $scope.SearchRef($scope.OrderList.brandid, $scope.BrandList);
            if (branddisplay && branddisplay != null && branddisplay != '') attributes.push(branddisplay);
            $scope.OrderList.attributedisplay = attributes.join('/');
            $scope.Jobwork.Jobworkbillitems.push(JSON.parse(JSON.stringify($scope.OrderList)));
            $scope.OrderList = { 'jobworkbillitemsid': 0 };
            $scope.CalculateTotal();
            $scope.setFocusbarcode();
        };

        $scope.ManageItem = function (type, item) {
            if (type == 'Edit') {
                $scope.OrderList = item;
                $scope.OrderList.productname = $scope.SearchRef(item.productid, $scope.ProductList);
                $scope.Jobwork.Jobworkbillitems.splice($scope.Jobwork.Jobworkbillitems.indexOf(item), 1);
            }
            else {
                $scope.Jobwork.Jobworkbillitems.splice($scope.Jobwork.Jobworkbillitems.indexOf(item), 1);
            }
            $scope.egrid.UpdateData($scope.Jobwork.Jobworkbillitems, '_id');
            $scope.CalculateTotal();
        };


        $scope.CalculateTotal = function () {
            $scope.Jobwork.taxamount = 0;
            $scope.Jobwork.discount = 0;
            $scope.Jobwork.grossamount = 0;
            $scope.Jobwork.totalamount = 0;
            $scope.Jobwork.qty = 0;
            $scope.Jobwork.materialcost = 0;
            $scope.Jobwork.materialsalevalue = 0;

            $.each($scope.Jobwork.Jobworkbillitems, function (k, v) {
                $scope.Jobwork.grossamount = $scope.GetNumber(v.chargegross) + $scope.GetNumber($scope.Jobwork.grossamount);
                $scope.Jobwork.taxamount = $scope.GetNumber(v.chargetax) + $scope.GetNumber($scope.Jobwork.taxamount);
                $scope.Jobwork.totalamount = $scope.GetNumber(v.chargetotal) + $scope.GetNumber($scope.Jobwork.totalamount);
                $scope.Jobwork.qty = $scope.GetNumber(v.qty) + $scope.GetNumber($scope.Jobwork.qty);

                $scope.Jobwork.materialcost += $scope.GetNumber(v.qty) * $scope.GetNumber(v.materialcost);
                $scope.Jobwork.materialsalevalue += $scope.GetNumber(v.qty) * $scope.GetNumber(v.materialsalevalue);
            });

            $scope.Jobwork.taxamount = $scope.Jobwork.taxamount.round(2);
            $scope.Jobwork.discount = $scope.Jobwork.discount.round(2);
            $scope.Jobwork.grossamount = $scope.Jobwork.grossamount.round(2);
            $scope.Jobwork.totalamount = $scope.Jobwork.totalamount.round(2);
            if ($scope.Jobwork.Jobworkbillitems.length == 1) {
                $scope.JobWorkEntryConfig();
            }
            $scope.egrid.UpdateData($scope.Jobwork.Jobworkbillitems, '_id');
        }

        $scope.SaveConfirmation = function () {
            $.confirm({
                title: 'Confirmation',
                content: 'Once Completed, This Job Work Cannot be Edited or Deleted.',
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
                            var request = {
                                "Item": (JSON.parse(JSON.stringify($scope.Jobwork))),
                                "securitytoken": "String content"
                            }
                            var srequest = JSON.stringify(request);

                            $http({
                                method: 'POST',
                                url: "JobworkbillgrnService/Save",
                                data: srequest
                            }).success(function (result) {

                                if (result != null && result.id > 0) {
                                    if (result.ErrorMessage == null || result.ErrorMessage == '')
                                        $rootScope.Toaster.Info("Job GRN Successfully Save with Invoice no : " + result.invoiceno + "/" + result.entryno);
                                    else
                                        $rootScope.Toaster.Error(result.ErrorMessage);

                                    var current = {
                                        'supplierid': request.Item.supplierid,
                                        'suppliercompanyid': request.Item.suppliercompanyid,
                                        'locationid': request.Item.locationid,
                                        'companyid': request.Item.companyid
                                    };
                                    $scope.Jobwork = (JSON.parse(JSON.stringify($scope.Entity)));
                                    $scope.Jobwork.supplierid = current.supplierid;
                                    $scope.Jobwork.suppliercompanyid = current.suppliercompanyid;
                                    $scope.Jobwork.locationid = current.locationid;
                                    $scope.Jobwork.companyid = current.companyid;
                                    $scope.JobWorkEntryConfig();
                                    $scope.egrid.UpdateData($scope.Jobwork.Jobworkbillitems, '_id');
                                    if (typeof ($location.search().ids) != 'undefined' &&
                                        $location.search().ids.length > 0) {
                                        $scope.SearchPending('', $location.search().ids.split(','));
                                    }
                                    else {
                                        $scope.SearchPending();
                                    }
                                }

                            }).error(function (result) {
                            });

                        }
                    },
                    cancel: function () {
                        //return false;
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

        $scope.Save = function () {

            if ($scope.Jobwork.invoiceno == null || $scope.Jobwork.invoiceno == '') {
                $rootScope.Toaster.Info("Please enter the invoice no.!");
                return;
            }

            if ($scope.Jobwork.companyid == null || $scope.Jobwork.companyid == 0) {
                $rootScope.Toaster.Info("Please Select Company.!");
                return;
            }

            if ($scope.Jobwork.supplierid == null || $scope.Jobwork.supplierid == 0) {
                $scope['setFocussupplierid']();
                $rootScope.Toaster.Info("Please Select Supplier.!");
                return;
            }

            if ($scope.Jobwork.status == 2 || $scope.Jobwork.status == '2') {
                $scope.SaveConfirmation();
                return;
            }

            var request = {
                "Item": (JSON.parse(JSON.stringify($scope.Jobwork))),
                "securitytoken": "String content"
            }
            var srequest = JSON.stringify(request);

            $http({
                method: 'POST',
                url: "JobworkbillgrnService/Save",
                data: srequest
            }).success(function (result) {

                if (result != null && result.id > 0) {
                    if (result.ErrorMessage == null || result.ErrorMessage == '')
                        $rootScope.Toaster.Info("Job GRN Successfully Save with Invoice no : " + result.invoiceno + "/" + result.entryno);
                    else
                        $rootScope.Toaster.Error(result.ErrorMessage);

                    var current = {
                        'supplierid': request.Item.supplierid,
                        'suppliercompanyid': request.Item.suppliercompanyid,
                        'locationid': request.Item.locationid,
                        'companyid': request.Item.companyid
                    };
                    $scope.Jobwork = (JSON.parse(JSON.stringify($scope.Entity)));
                    $scope.Jobwork.supplierid = current.supplierid;
                    $scope.Jobwork.suppliercompanyid = current.suppliercompanyid;
                    $scope.Jobwork.locationid = current.locationid;
                    $scope.Jobwork.companyid = current.companyid;
                    $scope.SearchPending();
                }

            }).error(function (result) {
            });
        }

        $scope.PrintBarcode = function () {
            var ids = [];
            var selectindex = 1;
            $.each(this.egrid.GetSelectedRows(), function (k, v) {
                var product = $scope.ProductList.find(function (item) {
                    return item.id == v.productid;
                });
                if (v.barcodes && v.barcodes != null && v.barcodes.length > 0) {

                    if (v.barcodes.length == 1) {
                        ids.push({
                            'barcode': v.barcodes[0],
                            'noofcopy': (product && product != null && product.sellingmode == 2) ? v.qty : 1,
                            'selectindex': selectindex++
                        });
                    }
                    else {
                        $.each(v.barcodes, function (bk, bv) {
                            ids.push({
                                'barcode': bv,
                                'noofcopy': 1,
                                'selectindex': selectindex++
                            });
                        })
                    }

                }
                else {
                    ids.push({
                        'barcode': v.barcode,
                        'noofcopy': (product && product != null && product.sellingmode == 2) ? v.qty : 1,
                        'selectindex': selectindex++
                    });
                }
            });
            if (ids.length == 0) {
                $rootScope.Toaster.Info("Please select barcode to be print.");
            }
            $scope.ChoseReport({
                'iswarehouse': false,
                'locationid': $scope.Jobwork.locationid,
                'barcodes': ids, 'reportid': 0
            });
        };

        $scope.ChoseReport = function (requestitem) {
            $rootScope.Handlers.SelectPrinter('BARCODE', function (printdata) {
                if (printdata == null || printdata.report == null || printdata.report.id <= 0)
                    return;

                requestitem.reportid = printdata.report.id;

                var request = {
                    "Item": requestitem,
                    "SecurityToken": "String content"
                }
                var srequest = JSON.stringify(request);
                $http({
                    method: 'POST',
                    url: "PricechangeService/PrintBarcode",
                    data: srequest
                }).success(function (result) {

                    if (result != null && result.length > 0) {
                        $scope.Sent2Printer(result, printdata);
                    }
                });
            });
        }

        $scope.Sent2Printer = function (printdocument, printerData) {
            $rootScope.Sent2Printer(printdocument.join(';'), printerData);
        }

        $scope.ManageSearchItem = function (action, item) {

            if (action == "Barcode") {
                $location.url('/barcode?invoiceids=' + item.lrinvoiceids);
                return;
            }

            if (action == "Delete") {
                $.confirm({
                    title: 'Delete',
                    content: 'Do you want to delete ' + item.name + "?",
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
                                    url: "JobworkbillgrnService/Delete",
                                    data: srequest
                                }).success(function (result) {

                                    if (result) {
                                        $scope.ErrorMessage = "Successfully deleted";
                                        $scope.SearchResult.splice($scope.SearchResult.indexOf(item), 1);
                                        $scope.pgrid.UpdateData($scope.SearchResult, '_id');
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
                    url: "JobworkbillgrnService/Search",
                    data: srequest
                }).success(function (result) {
                    $scope.Jobwork = result[0];
                    if ($scope.Jobwork.status == 2 || $scope.Jobwork.status == '2') {
                        $scope.Disable2ChangeStatus = true;
                    }
                    else {
                        $scope.Disable2ChangeStatus = false;
                    }
                    $scope.SearchPending($scope.Jobwork.id);
                    $scope.Navigation.SetMode('EDIT');

                    var newbarcodegenerates = $scope.Jobwork.Jobworkbillitems.find(function (item) {
                        return (item.barcodes && item.barcodes != null && item.barcodes.length > 0);
                    });

                    $scope.JobWorkEntryConfig(newbarcodegenerates && newbarcodegenerates != null);
                    $scope.egrid.UpdateData($scope.Jobwork.Jobworkbillitems, '_id');
                }).error(function (result) {
                });
            }
        }

        $scope.Disable2ChangeStatus = false;

        $scope.Search = function () {
            $scope.ShowSearch = true;

            request = {
                "Item": $scope.SearchCondition,
                "securitytoken": "String content"
            }

            var srequest = JSON.stringify(request);
            $http({
                method: 'POST',
                url: "JobworkbillgrnService/Search",
                data: srequest
            }).success(function (result) {
                $scope.SearchResult = result;
                $scope.JobWorkConfig();
                if (result == 'undefined' || result == null || result.length == 0)
                    $rootScope.Toaster.Info("No Results Found.");
            }).error(function (result) {
            });
        }

        $scope.egrid = null;
        $scope.JobWorkEntryConfig = function (showgenbarcodes) {
            if ($scope.egrid != null)
                $scope.egrid.destroy();

            var igrid = new InfyGrid('editGrid', '450px', '100%', true, true, true, 140);
            igrid.options.showHeaderRow = true;
            igrid.AddSimpleColumn('barcode', 'Barcode', 100);
            if (showgenbarcodes) {
                igrid.AddCustomColumn('barcodes', 'New Barcodes', 120, function (item) {
                    if (item.barcodes && item.barcodes != null && item.barcodes.length > 0)
                        return item.barcodes.join(',');
                    return '';
                });
            }
            igrid.AddReferenceColumn('productid', 'Product', 150, $scope.ProductList);
            igrid.AddSimpleColumn('designid', 'Design', 120);
            igrid.AddSimpleColumn('attributedisplay', 'Detail', 200);
            if ($scope.system_invcol_colour)
                igrid.AddReferenceColumn('colourid', 'Colour', 120, $scope.ColourList);
            if ($scope.system_invcol_size)
                igrid.AddReferenceColumn('sizeid', 'Size', 120, $scope.SizeList);
            igrid.AddFormatedColumn('qty', 'Qty', 75, 'N2', 2);
            igrid.AddFormatedColumn('chargegross', 'Rate', 75, 'N2', 2);
            igrid.AddFormatedColumn('chargetotal', 'Amount', 75, 'N2', 2);
            igrid.AddFormatedColumn('finalprice', 'Selling Pirce', 75, 'N2', 2);
            igrid.AddButtonColumn('Delete', '', 35, "<button type='button' style='width:30px;height:25px;margin-right:5px;line-height: 0;' value='Delete' class='btn btn-danger btn-xs'><i class='fa fa-trash' style='font:normal normal normal 14px/1 FontAwesome'></i></button>");
            igrid.AddButtonColumn('Edit', '', 35, "<button type='button' style='width:30px;height:25px;margin-right:5px;line-height: 0;' value='Edit' class='btn btn-info btn-xs'><i class='fa fa-edit' style='font:normal normal normal 14px/1 FontAwesome'></i></button>");

            igrid.options.autoEdit = true;
            igrid.Initialize();

            igrid.grid.onClick.subscribe(function (e) {
                var cell = igrid.grid.getCellFromEvent(e);
                var item = igrid.grid.getDataItem(cell.row);
                var command = igrid.grid.getColumns()[cell.cell].id;
                $scope.$apply(function () {
                    if (command == 'Edit' || command == 'Delete')
                        $scope.ManageItem(command, item);
                });
            });
            $scope.egrid = igrid;
            $scope.egrid.UpdateData([]);
        }

        $scope.JobWorkConfig = function () {
            if ($scope.pgrid != null)
                $scope.pgrid.destroy();

            var pgrid = new InfyGrid('gridContainer', '450px', '100%', true, false, true, 20);
            pgrid.options.showHeaderRow = true;
            pgrid.EnableSerial($scope.SearchResult);
            pgrid.AddSimpleColumn('entryno', 'Entry No', 100);
            pgrid.AddSimpleColumn('invoiceno', 'GRN No', 100);
            pgrid.AddFormatedColumn('invoicedate', 'Invoice Date', 85, 'dd-MM-yyyy');
            pgrid.AddReferenceColumn('supplierid', 'Job Worker', 170, $scope.Suppliers);
            pgrid.AddReferenceColumn('locationid', 'Location', 120, $scope.Locations);
            pgrid.AddFormatedColumn('qty', 'No Of Pieces', 80, 'N2', 2).total = true;
            pgrid.AddFormatedColumn('totalamount', 'Total Amt', 100, 'N2', 2).total = true;
            pgrid.AddReferenceColumn('status', 'Status', 100, $scope.Status);

            var commands = "<input type='button' class='btn btn-secondary btn-xs' value='Barcode'/> &nbsp;";
            if ($rootScope.UserInfo.p_STOREJOBWORKGRN.modify)
                commands += "<input type='button' class='btn btn-primary btn-xs' value='Edit'/> &nbsp;"
            if ($rootScope.UserInfo.p_STOREJOBWORKGRN.delete)
                commands += "<input type='button' class='btn btn-danger btn-xs' value='Delete'/> &nbsp;"

            if (commands != '') {
                pgrid.AddButtonColumn('action', 'Action', 200, commands, function (e, args) {
                    $scope.$apply(function () {
                        $scope.ManageSearchItem($(e.target).val(), args.item);
                    });
                    return true;
                });
            }
            pgrid.options.autoEdit = true;
            pgrid.Initialize();
            $scope.pgrid = pgrid;
            $scope.pgrid.UpdateData($scope.SearchResult);
        }

        $scope.CreateMultiItems = function (ItemDetails, isJump, isView, isSizeGroup, isShowChange) {
            var dialog = ngDialog.open({
                template: 'MultiSize',
                data: {
                    'ItemDetails': ItemDetails,
                    'SearchRef': $scope.SearchRef,
                    'SizeList': $scope.SizeList,
                    'isJump': true,
                    'isView': false,
                    'scope': $scope,
                    'isSizeGroup': false
                },
                width: '750px',
                controller: ['$scope', '$timeout', 'restService', function ($scope, $timeout, restService) {
                    $scope.SupportUANPurchase = $scope.ngDialogData.scope.SupportUANPurchase;
                    $scope.ProductSearch = $scope.ngDialogData.scope.ProductSearch;
                    $scope.IsJump = $scope.ngDialogData.isJump;
                    $scope.SupportSalepriceMrpdifference = true;
                    $scope.isSizeGroup = $scope.ngDialogData.isSizeGroup;
                    $scope.sizeInfo = '';
                    $scope.SizeGroupList = $scope.ngDialogData.scope.SizeGroupList;
                    $scope.system_hidecostinventory = $scope.ngDialogData.scope.system_hidecostinventory;
                    $scope.isView = $scope.ngDialogData.isView;
                    $scope.RetialLocations = $scope.ngDialogData.scope.RetialLocations;
                    $scope.CollectLocationInfo = $scope.ngDialogData.scope.CollectLocationInfo;
                    $scope.stocklocationid = $scope.ngDialogData.ItemDetails.stocklocationid;
                    $scope.Created = 0;
                    $scope.ParentScope = $scope.ngDialogData.scope;
                    $scope.SupportFinalPriceChange = $scope.ParentScope.SupportFinalPriceChange;
                    $scope.SupportRoundingDiscount = $scope.ParentScope.SupportRoundingDiscount;
                    $scope.AllowChangePrice = typeof (isShowChange) != 'undefined' && isShowChange;
                    $scope.CoreItem = $scope.ngDialogData.ItemDetails;
                    $scope.TotalAmount = 0;
                    $scope.Total = $scope.CoreItem.qty;
                    $scope.PopupList = [];
                    $scope.CanClear = true;
                    $timeout(function () {
                        if ($scope.IsJump) {
                            $scope['setFocussizeInfo']();
                        } else {
                            $scope['setFocussizeInfoBit']();
                        }
                    }, 100);


                    $scope.finalpricechanged = function (item) {
                        var fp = $scope.ParentScope.GetNumber(item.finalprice);
                        item.salediscount = $scope.ParentScope.GetNumber(item.sellingprice) - fp;
                        if (item.salediscount != 0)
                            item.salediscountpercentage = (item.salediscount / item.sellingprice * 100).round(2);
                        else
                            item.salediscountpercentage = 0;
                    }

                    $scope.salesdiscountchanged = function (item) {
                        var sd = $scope.ParentScope.GetNumber(item.salediscount);
                        if (sd != 0)
                            item.salediscountpercentage = (item.salediscount / item.sellingprice * 100).round(2);
                        else
                            item.salediscountpercentage = 0;
                        item.finalprice = item.sellingprice - $scope.ParentScope.GetNumber(item.salediscount);
                    }

                    $scope.salesdiscountpercentagechanged = function (item) {
                        var sd = $scope.ParentScope.GetNumber(item.salediscountpercentage);
                        if (sd != 0)
                            item.salediscount = ((item.sellingprice * sd) / 100).round(2);
                        else
                            item.salediscount = 0;
                        item.finalprice = item.sellingprice - $scope.ParentScope.GetNumber(item.salediscount);
                    }


                    $scope.CreateItems = function ($event) {
                        $scope.isHideIncrese = false;
                        if (!$scope.isSizeGroup &&
                            (typeof ($scope.sizeInfo) == 'undefined' || $scope.sizeInfo == null || $scope.sizeInfo.toString() == '')) {
                            if (!$scope.Selected()) {
                                if ($scope.PopupList.length > 0) {
                                    $scope.setFocusmcell_firstrow();
                                } else {
                                    if ($scope.IsJump) {
                                        $scope['setFocussizeInfo']();
                                    } else {
                                        $scope['setFocussizeInfoBit']();
                                    }
                                }
                                $event.preventDefault();
                            }
                            return true;
                        }

                        var cellid = 0;

                        var sizeidList = $scope.sizeInfo.toString().split(',');
                        var sizeList = sizeidList[0].split('-');
                        if (sizeList.length == 2 && /^[a-zA-Z2-6]*$/.test(sizeList[0])) {

                            var sizeIndex = '3XS,XXS,XS,S,M,L,XL,XXL,XXXL,4XL,5XL,6XL,7XL'.split(',');
                            var startsize = sizeIndex.indexOf(sizeList[0]);
                            var endsize = sizeIndex.indexOf(sizeList[1]);
                            if (startsize < 0 || endsize < 0) {
                                sizeIndex = '3XS,XXS,XS,S,M,L,XL,2XL,3XL,4XL,5XL,6XL,7XL'.split(',');
                                startsize = sizeIndex.indexOf(sizeList[0]);
                                endsize = sizeIndex.indexOf(sizeList[1]);
                            }
                            sizeList = [];
                            for (var index = startsize; index <= endsize; index++) {
                                sizeList.push(sizeIndex[index]);
                            }

                        }
                        else if (sizeList.length >= 3)
                        {
                            var firstPart = sizeList[0].match(/[^-_ \d]+|\d+/g);
                            var lastPart = sizeList[2].match(/[^-_ \d]+|\d+/g);
                            var suffix = '';
                            var startsize = parseInt(sizeList[0]);
                            var increment = parseInt(sizeList[1]);
                            var endsize = parseInt(sizeList[2]);

                            if (firstPart.length == 2 && /^[a-zA-Z2-6]*$/.test(firstPart[1]))
                                suffix = firstPart[1];

                            sizeList = [];
                            for (; startsize <= endsize; startsize = startsize + increment) {
                                var searchsize = startsize.toString() + suffix;

                                var excluded = false;
                                for (var i = 1; i < sizeidList.length; i++) {
                                    if (sizeidList[i] == searchsize)
                                        excluded = true;
                                }
                                if (!excluded)
                                    sizeList.push(searchsize);
                            }
                        }

                        var idList = [];
                        $.each($scope.ngDialogData.SizeList, function (k, v) {
                            $.each(sizeList, function (kk, vv) {
                                if (vv == v.name) {
                                    if (idList.find(function (item) {
                                        return item.name == v.name;
                                    }) == null) {
                                        idList.push({
                                            'id': v.id,
                                            'name': v.name,
                                            'qty': 0,
                                            'index': kk
                                        });
                                    }
                                }
                            });
                        });


                        if (idList.length > 0) {

                            idList.sort(function (a, b) {
                                return a.index - b.index;
                            });

                            var qty = Math.floor(($scope.CoreItem.qty - $scope.Created) / idList.length);
                            if (!isNaN(qty)) {
                                $.each(idList, function (k, v) {
                                    var nitem = {
                                        'sizename': v.name,
                                        'sizeid': v.id,
                                        'qty': qty,
                                        'sizegroup': $scope.sizeInfo,
                                        'finalprice': $scope.GetNumber($scope.CoreItem.finalprice),
                                        'dealerprice': $scope.GetNumber($scope.CoreItem.dealerprice),
                                    }
                                    $scope.TotalAmount += (nitem.qty * nitem.finalprice).round(2);
                                    if ($scope.PopupList.length == 0) {
                                        nitem.cellid = 'mcell_firstrow';
                                    }
                                    $scope.PopupList.push(nitem);
                                    $scope.Created += qty;
                                });
                                $scope.Created = $scope.Created.round(2);
                                if ($scope.Created < qty)
                                    $scope.PopupList[$scope.PopupList.length - 1].qty = qty - $scope.Created;
                            }
                        }
                    };


                    $scope.GetNumber = function (value) {
                        if (typeof (value) == 'number')
                            return value;

                        if (typeof (value) == 'undefined' || value == '')
                            return 0;

                        nvalue = parseFloat(value);
                        if (isNaN(nvalue))
                            return 0;

                        return nvalue;
                    };

                    $scope.QtyChanged = function (item, newVal, oldVal) {

                        var oldvalue = parseFloat(oldVal);
                        var newvalue = parseFloat(newVal);

                        if (isNaN(oldvalue)) {
                            oldvalue = 0;
                        }
                        if (isNaN(newvalue)) {
                            newvalue = 0;
                        }
                        if (typeof (item.sellableqty) != 'undefined' && item.sellableqty > 0 && item.sellableqty > newvalue) {
                            item.qty = item.sellableqty;
                            return;
                        }

                        $scope.TotalAmount -= (oldvalue * item.buyingprice).round(2)
                        $scope.TotalAmount += (newvalue * item.buyingprice).round(2)
                        $scope.Created -= oldvalue;
                        $scope.Created += newvalue;
                        $scope.Created = $scope.Created.round(2);
                        //$scope.PriceChanged(item);
                    };

                    $scope.Delete = function (item) {
                        if (typeof (item.sellableqty) != 'undefined' && item.sellableqty > 0) {
                            $rootScope.Toaster.Error("Barcode already generated for this item, so can't delete");
                            return;
                        }
                        $scope.PopupList.splice($scope.PopupList.indexOf(item), 1);
                        $scope.TotalAmount -= (item.qty * item.buyingprice).round(2)
                        $scope.Created -= item.qty;
                        $scope.Created = $scope.Created.round(2);
                    };

                    $scope.Clear = function () {
                        $scope.PopupList = [];
                        $scope.Created = 0;
                        $scope.TotalAmount = 0;
                    }

                    $scope.Selected = function () {

                        if ($scope.Created != $scope.Total || $scope.Created == 'undefined' || $scope.Created == null) {
                            $rootScope.Toaster.Error("Check Qty..!");

                            return ($scope.Created == $scope.Total);
                        } else {
                            $scope.closeThisDialog({
                                'list': $scope.PopupList,
                                'stocklocationid': $scope.stocklocationid
                            });
                            return true;
                        }
                    };

                }],
                className: 'ngdialog-theme-plain'
            });
            dialog.closePromise.then(function (data) {
                {
                    if (typeof (data.value) != 'undefined' && data.value != null && data.value != '$escape' && data.value != '$document') {
                        var stocklocationid = 0;
                        var cloneDetails = (JSON.parse(JSON.stringify($scope.OrderList)));
                        cloneDetails.id = 0;
                        cloneDetails.stocklocationid = stocklocationid;
                        var taxitem = $scope.CurrentPurchaseTaxConfig;
                        var salestaxitem = $scope.CurrentSalesTaxConfig;
                        $.each(data.value.list, function (k, v) {
                            try {
                                $scope.OrderList = (JSON.parse(JSON.stringify(cloneDetails)));
                                $scope.OrderList.sizeid = v.sizeid;
                                $scope.OrderList.qty = $scope.GetNumber(v.qty);
                                $scope.OrderList.chargegross = $scope.OrderList.qty * $scope.OrderList.chargerate;
                                $scope.OrderList.finalprice = $scope.GetNumber(v.finalprice);
                                $scope.OrderList.dealerprice = $scope.GetNumber(v.dealerprice);
                                $scope.OrderList.chargetax = ($scope.GetNumber($scope.OrderList.chargegross) * $scope.GetNumber($scope.OrderList.chargetaxpercentage) / 100).round(2);
                                if (isNaN($scope.OrderList.chargetax)) $scope.OrderList.chargetax = 0;
                                $scope.OrderList.chargetotal = ($scope.GetNumber($scope.OrderList.chargegross) + $scope.GetNumber($scope.OrderList.chargetax)).round(0);
                                $scope.AddItem();
                            } catch (e) {
                                e = null;
                            }
                        });
                    }
                }
            });
        };

        $scope.ClearItems = function () {
            $scope.Jobwork.Jobworkbillitems = [];
            $scope.CalculateTotal();
        }

        $scope.SizeBasedFocus = function () {
            if ($scope.system_invcol_colour) {
                $scope.setFocuscolourid();
            }
            else {
                $scope.setFocussizeid();
            }
        }

        $scope.Appsetting_Company = false;
        $scope.system_invcol_colour = false;
        $scope.system_invcol_size = false;
        $scope.system_b2bexclusivetax = false;
        $scope.SupportImageRecording = false;
        $scope.system_jobworkcostmanual = false;
        $scope.LoadApplicationSetting = function () {
            if (typeof ($rootScope.UserInfo) != 'undefined' &&
                typeof ($rootScope.UserInfo.appsettings) != 'undefined' &&
                $rootScope.UserInfo.appsettings != null) {
                $scope.Appsetting_Company = true;
                $scope.system_invcol_colour = $rootScope.UserInfo.appsettings.system_invcol_colour || false;
                $scope.system_invcol_size = $rootScope.UserInfo.appsettings.system_invcol_size || false;
                $scope.system_b2bexclusivetax = $rootScope.UserInfo.appsettings.system_b2bexclusivetax || false;
                $scope.SupportImageRecording = $rootScope.UserInfo.appsettings.system_supportimagerecord || false;
                $scope.system_jobworksamebarcode = $rootScope.UserInfo.appsettings.system_jobworksamebarcode || false;
                $scope.system_jobworkcostmanual = $rootScope.UserInfo.appsettings.system_jobworkcostmanual || false;
                $scope.JobWorkEntryConfig();
            }
            else {
                $timeout(function () {
                    $scope.LoadApplicationSetting();
                }, 50);
            }
        }

        $timeout(function () {
            $scope.LoadApplicationSetting();
            $scope.setFocusfirst();
            if (typeof ($location.search().ids) != 'undefined' &&
                $location.search().ids.length > 0) {
                $scope.SearchPending('', $location.search().ids.split(','));
            }
        }, 10);

    }]);

