var app = angular.module('InfinityLab');

app.controller('MissingStockController', ['UserDetails', '$scope', '$http', '$location', '$rootScope', '$window', 'restService', 'Excel', '$timeout', '$filter',
    function (UserDetails, $scope, $http, $location, $rootScope, $window, restService, Excel, $timeout, $filter) {
        $rootScope.AccordionName = $location.path();
        $scope.SearchRef = restService.SearchReference;
        $scope.SupportDealerPrice = false;
        $scope.SupportMarkDOWN = false;
        $scope.SupportMarginexcludetax = false;
        $scope.SupportRounding = false;
        $scope.system_ismfgapplication = false;
        $scope.System_b2brounding = 0;
        $scope.status = '';
        $scope.LoadingConfiguration = function () {
            if (typeof ($rootScope.UserInfo.appsettings) == 'undefined' || $rootScope.UserInfo.appsettings == null) {
                $timeout($scope.LoadingConfiguration, 100);
            }
            else {

                if (false || $scope.UserInfo.appsettings.system_b2brounding)
                    $scope.System_b2brounding = 4;
                $scope.system_ismfgapplication = $rootScope.UserInfo.appsettings.system_ismfgapplication || false; 
                $scope.SupportRounding = $rootScope.UserInfo.appsettings.system_supportrounding || false;
                $scope.SupportMarginexcludetax = $rootScope.UserInfo.appsettings.system_marginexcludetax || false;
                $scope.SupportDealerPrice = $rootScope.UserInfo.appsettings.system_dealerprice || false;
                $scope.SupportMarkDOWN = $rootScope.UserInfo.appsettings.system_marginonmarkup || $rootScope.UserInfo.appsettings.system_marginonmarkdown || false;
            }
        }

        $scope.LoadingConfiguration();
        $scope.BuyingCompany = restService.Get('ReferencelistService/BUYINGCOMPANY', true).then(function (results) {
            $scope.BuyingCompany = results;
        });
        $scope.Location = restService.Get('ReferencelistService/ALLLOCATION', true).then(function (results) {
            $scope.Location = results;
        });
        $scope.Suppliers = restService.Get('ReferencelistService/SUPPLIER', true).then(function (results) {
            $scope.Suppliers = results;
        });

        $scope.Products = restService.Get('ReferencelistService/PRODUCTS', true).then(function (results) {
            $scope.Products = results;
        });

        $scope.Brands = restService.Get('ReferencelistService/BRAND', true).then(function (results) {
            $scope.Brands = results;
        });

        $scope.ColourList = restService.Get('ReferencelistService/COLOUR', true).then(function (results) {
            $scope.ColourList = results;
        });

        $scope.StyleList = restService.Get('ReferencelistService/STYLE', true).then(function (results) {
            $scope.StyleList = results;
        });


        $scope.Suppliers = restService.Get('ReferencelistService/SUPPLIER', true).then(function (results) {
            $scope.Suppliers = results;
        });

        $scope.PurchaseTax = restService.Get('ReferencelistService/PURCHASETAX', true).then(function (results) {
            $scope.PurchaseTax = results;
        });

        $scope.Sizes = restService.Get('ReferencelistService/SIZE', true).then(function (results) {
            $scope.Sizes = results;
        });

        $scope.AllSizeGroup = true;
        $scope.SizeGroups = restService.Get('ReferencelistService/SIZEGROUP', true).then(function (results) {
            $scope.SizeGroups = results;
            $scope.AllSizeGroup = $rootScope.UserInfo.appsettings.system_supportsizegroup;
        });

        $scope.SearchResult = [];
        $scope.selectedcount = 0;
        $scope.item = { 'sizegroup': [] };

        $scope.DPPercentageChanged = function () {
            if (typeof ($scope.item.dealerprice) != 'undefined' &&
                $scope.item.dealerprice != null &&
                ($scope.item.dealerprice.toString().indexOf('%') >= 0 || $scope.item.dealerprice.toString().indexOf('*') >= 0)) {

                var marginpercentage = $scope.GetNumber($scope.item.dealerprice.toString().replace('%', '').replace('*', ''));

                if (isNaN(marginpercentage))
                    marginpercentage = 0;

                var baseprice = parseFloat($scope.item.buyingprice);
                if (isNaN(baseprice))
                    baseprice = 0;
                if (baseprice > 0 && marginpercentage > 0) {
                    if ($scope.SupportMarkDOWN) {
                        var markpercent = 1 + (marginpercentage / (100 - marginpercentage));
                        $scope.item.dealerprice = (baseprice * markpercent).round($scope.System_b2brounding);
                    }
                    else {
                        $scope.item.dealerprice = baseprice + (baseprice * marginpercentage / 100).round($scope.System_b2brounding);
                    }
                }

                if ($scope.SupportMarginexcludetax && $scope.item.taxitem != null) {
                    var taxconfig = $scope.item.taxitem.taxsplit.findTax($scope.GetNumber($scope.item.dealerprice));
                    var newtaxvalue = $scope.GetNumber($scope.item.dealerprice) * (taxconfig.cgst + taxconfig.sgst) / 100;
                    newdealerprice = (newtaxvalue + $scope.item.dealerprice).round($scope.System_b2brounding);
                    $scope.item.dealerprice = newdealerprice;
                }
            }
            $scope.setFocusdisplaymrp();
        }

        $scope.DeleteSelected = function () {
            var items = [];
            $.each(this.igrid.GetSelectedRows(), function (k, v) {
                items.push(v);
            });
            if (items.length == 0) return;
            $.each(items, function (k, v) {
                $scope.SearchResult.splice($scope.SearchResult.indexOf(v), 1);
            });
            $.each($scope.SearchResult, function (k, v) {
                v.sno = k + 1;
            });
            $scope.igrid.UpdateData($scope.SearchResult);
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

        $scope.MRPPercentageChanged = function () {
            if (typeof ($scope.item.displaymrp) != 'undefined' &&
                $scope.item.displaymrp != null &&
                ($scope.item.displaymrp.toString().indexOf('%') >= 0 || $scope.item.displaymrp.toString().indexOf('*') >= 0)) {

                var marginpercentage = $scope.GetNumber($scope.item.displaymrp.toString().replace('%', '').replace('*', ''));

                if (isNaN(marginpercentage))
                    marginpercentage = 0;

                var baseprice = parseFloat($scope.item.price);
                if (isNaN(baseprice))
                    baseprice = 0;
                if (baseprice > 0 && marginpercentage > 0) {
                    if ($scope.SupportMarkDOWN) {
                        var markpercent = 1 + (marginpercentage / (100 - marginpercentage));
                        $scope.item.displaymrp = (baseprice * markpercent).round(0);
                    }
                    else {
                        $scope.item.displaymrp = baseprice + (baseprice * marginpercentage / 100).round(0);
                    }
                }

                if ($scope.SupportMarginexcludetax && $scope.item.taxitem != null) {
                    var taxconfig = $scope.item.taxitem.taxsplit.findTax($scope.GetNumber($scope.item.displaymrp));
                    var newtaxvalue = $scope.GetNumber($scope.item.displaymrp) * (taxconfig.cgst + taxconfig.sgst) / 100;
                    newdisplaymrp = $scope.RoundPrice(newtaxvalue + $scope.item.displaymrp);
                    $scope.item.displaymrp = newdisplaymrp;
                }
            }
            $scope.setFocusbtnmrp();

        }

        $scope.PriceValidation = function () {
            if (typeof ($scope.item.price) != 'undefined' &&
                $scope.item.price != null &&
                ($scope.item.price.toString().indexOf('%') >= 0 || $scope.item.price.toString().indexOf('*') >= 0)) {
                var marginpercentage = $scope.GetNumber($scope.item.price.toString().replace('%', '').replace('*', ''));
                if (isNaN(marginpercentage))
                    marginpercentage = 0;

                var baseprice = parseFloat($scope.item.buyingprice);
                if (isNaN(baseprice))
                    baseprice = 0;
                if (baseprice > 0 && marginpercentage > 0) {
                    if ($scope.SupportMarkDOWN) {
                        var markpercent = 1 + (marginpercentage / (100 - marginpercentage));
                        $scope.item.price = (baseprice * markpercent).round(0);
                    }
                    else {
                        $scope.item.price = baseprice + (baseprice * marginpercentage / 100).round(0);
                    }
                }

                if ($scope.SupportMarginexcludetax && $scope.item.taxitem != null) {
                    var taxconfig = $scope.item.taxitem.taxsplit.findTax($scope.GetNumber($scope.item.price));
                    var newtaxvalue = $scope.GetNumber($scope.item.price) * (taxconfig.cgst + taxconfig.sgst) / 100;
                    newprice = $scope.RoundPrice(newtaxvalue + $scope.item.price);
                    $scope.item.price = newprice;
                }
            }
            $scope.setFocusdiscount();
        }

        $scope.RoundPrice = function (amount) {

            if (!$scope.SupportRounding) return Math.round(amount, 0).toFixed(0);

            if (amount < 100) {
                amount = Math.round(amount, 0);
            }

            if (amount > 100 && amount < 1000) {
                amount = Math.round(amount, 0);
                var diff = amount - (Math.floor(amount / 10) * 10);
                if (diff > 5) {
                    if (diff - 5 >= 2.5) {
                        amount = (Math.floor(amount / 10) * 10) + 10;
                    }
                    else {
                        amount = (Math.floor(amount / 10) * 10) + 5;
                    }
                }
                else {
                    if (diff >= 2.5) {
                        amount = (Math.floor(amount / 10) * 10) + 5;
                    }
                    else {
                        amount = (Math.floor(amount / 10) * 10);
                    }
                }

            } else if (amount > 1000) {
                amount = Math.round(amount, 0);
                var diff = amount - (Math.floor(amount / 10) * 10);
                if (diff > 5) {
                    amount = (Math.floor(amount / 10) * 10) + 10;
                }
                else {
                    amount = (Math.floor(amount / 10) * 10);
                }
            }
            return amount;
        }

        $scope.DPHsnValidation = function () {
            if ($scope.SupportDealerPrice)
                $scope.setFocusdphsncode();
            else
                $scope.setFocushsncode();
        }

        $scope.CanAddItem = function () {
            if ($scope.SupportDealerPrice)
                $scope.setFocuspdealerprice();
            else
                $scope.AddItems();
        }

        $scope.AddItems = function () {
            $scope.item.qty = $rootScope.GetNumber($scope.item.qty);
            $scope.item.buyingprice = $rootScope.GetNumber($scope.item.buyingprice);
            $scope.item.price = $rootScope.GetNumber($scope.item.price);
            $scope.item.discountpercentage = $rootScope.GetNumber($scope.item.discountpercentage);
            $scope.item.discount = $rootScope.GetNumber($scope.item.discount);
            $scope.item.dealerprice = $rootScope.GetNumber($scope.item.dealerprice);
            $scope.item.displaymrp = $rootScope.GetNumber($scope.item.displaymrp);

            if ($scope.item == 'undefined' || $scope.item == null) {
                $rootScope.Toaster.Info("Please enter the relavent details!");
                return;
            }
            if ($scope.item.locationid == 'undefined' || $scope.item.locationid == null
                || $scope.item.locationid == 0) {
                $rootScope.Toaster.Info("Please select the location!");
                return;
            }
            if ($scope.item.productid == 'undefined' || $scope.item.productid == null || $scope.item.productid == 0) {
                $rootScope.Toaster.Info("Please select the product!");
                return;
            }

            if ($scope.item.companyid == 'undefined' || $scope.item.companyid == null || $scope.item.companyid == 0) {
                $rootScope.Toaster.Info("Please select the company!");
                return;
            }

            var location = $scope.Location.find(function (item) {
                return item.id == $scope.item.locationid;
            });

            var company = $scope.BuyingCompany.find(function (item) {
                return item.id == $scope.item.companyid;
            });

            var product = $scope.Products.find(function (item) {
                return item.id == $scope.item.productid;
            });

            if (product.prodcompanyid > 0 && company.id != product.prodcompanyid) {
                $rootScope.Toaster.Info("Selected product can't be added to this company " + company.name);
                return;
            }

            if (location.additionalinfo != null &&
                location.additionalinfo.parentid > 0 && company.id != location.additionalinfo.parentid) {
                $rootScope.Toaster.Info("Selected location can't sell on this company " + company.name);
                return;
            }

            if ($scope.item.qty == 0 || $scope.item.price == 0) {
                $rootScope.Toaster.Info("Please Enter Qty and Selling Price");
                return;
            }
            if ($scope.item.buyingprice == 'undefined' || $scope.item.buyingprice == null || $scope.item.buyingprice == 0) $scope.item.buyingprice = $scope.item.price;


            if ($scope.system_ismfgapplication && $scope.item.serialno && $scope.item.serialno != null && $scope.item.serialno != '') {
                $scope.item.barcode = $scope.item.serialno;
                $scope.item.serialno = '';
            }

            if (typeof ($scope.item.barcode) == 'undefined' || $scope.item.barcode == null) {

                var request = {
                    "Item": {
                        'productid': $scope.item.productid,
                        'companyid': $scope.item.companyid,
                        'locationid': $scope.item.locationid,
                        'qty': $scope.item.qty
                    },
                    "SecurityToken": "String content"
                }
                var srequest = JSON.stringify(request);

                $http({
                    method: 'POST',
                    url: "StockService/CreateBarcode",
                    data: srequest
                }).success(function (result) {

                    if (result.barcodes != 'undefined' && result.barcodes != null && result.barcodes.indexOf(",") > 0) {
                        var barcodelist = result.barcodes.split(",");
                        $.each(barcodelist, function (k, v) {
                            var sitem = (JSON.parse(JSON.stringify($scope.item)));
                            
                            sitem.barcode = v;
                            sitem.qty = 1;
                            sitem.sellingname = $scope.SearchRef($scope.item.productid, $scope.Products);
                            sitem.productname = $scope.item.sellingname;
                            sitem.sizename = $scope.SearchRef($scope.item.sizeid, $scope.Sizes);
                            sitem.brandname = $scope.SearchRef($scope.item.brandid, $scope.Brands);
                            sitem.sno = $scope.SearchResult.length + 1;
                            $scope.SearchResult.push(sitem);

                        });
                        $scope.item = {
                            'sizegroup': [], 'companyid': $scope.item.companyid, 'locationid': $scope.item.locationid,
                            'displayinvoiceno': $scope.item.displayinvoiceno, 'displayinvoicedate': $scope.item.displayinvoicedate,
                            'supplierid': $scope.item.supplierid, 'productid': $scope.item.productid, 'hsncode': $scope.item.hsncode
                        };
                    }
                    else {
                        $scope.item.barcode = result.barcode;
                        $scope.item.sellingname = $scope.SearchRef($scope.item.productid, $scope.Products);
                        $scope.item.productname = $scope.item.sellingname;
                        $scope.item.sizename = $scope.SearchRef($scope.item.sizeid, $scope.Sizes);
                        $scope.item.brandname = $scope.SearchRef($scope.item.brandid, $scope.Brands);
                        $scope.item.sno = $scope.SearchResult.length + 1;
                        $scope.SearchResult.push($scope.item);
                        $scope.item.sno = $scope.SearchResult.length;
                        $scope.item = {
                            'sizegroup': [], 'companyid': $scope.item.companyid, 'locationid': $scope.item.locationid,
                            'displayinvoiceno': $scope.item.displayinvoiceno, 'displayinvoicedate': $scope.item.displayinvoicedate,
                            'supplierid': $scope.item.supplierid, 'productid': $scope.item.productid, 'hsncode': $scope.item.hsncode
                        };
                    }
                    if ($scope.SearchResult.length <= 1)
                        $scope.DefineGrid();
                    $scope.igrid.UpdateData($scope.SearchResult, '_id');
                }).error(function (result) {
                });
            }
            else {
                $scope.item.sellingname = $scope.SearchRef($scope.item.productid, $scope.Products);
                $scope.item.productname = $scope.item.sellingname;
                $scope.item.sizename = $scope.SearchRef($scope.item.sizeid, $scope.Sizes);
                $scope.item.brandname = $scope.SearchRef($scope.item.brandid, $scope.Brands);
                $scope.item.colourname = $scope.SearchRef($scope.item.colourid, $scope.ColourList);
                $scope.item.sno = $scope.SearchResult.length + 1;
                $scope.SearchResult.push($scope.item);
                $scope.item = {
                    'sizegroup': [], 'companyid': $scope.item.companyid, 'locationid': $scope.item.locationid,
                    'displayinvoiceno': $scope.item.displayinvoiceno, 'displayinvoicedate': $scope.item.displayinvoicedate,
                    'supplierid': $scope.item.supplierid, 'productid': $scope.item.productid, 'hsncode': $scope.item.hsncode
                };
                if ($scope.SearchResult.length <= 1)
                    $scope.DefineGrid();
                $scope.igrid.UpdateData($scope.SearchResult, '_id');
            }
            $scope.setFocusdesignid();
            $scope.status = 'New Item Added.';
        }


        $scope.CheckLocation = function () {
            return $rootScope.LocationInfo != null;
        }

        $scope.ProductChanged = function () {
            $scope.item.taxid = 0;
            $scope.item.taxitem = null;
            var product = $scope.Products.find(function (item) {
                return item.id == $scope.item.productid;
            });
            if (product && product != null) {
                $scope.item.hsncode = product.hsncode;
                $scope.item.taxid = product.salestaxid;
            };

            var taxitem = $scope.PurchaseTax.find(function (item) {
                return (item.id == $scope.item.taxid);
            });
            if (taxitem || taxitem != null) {
                $scope.item.taxitem = taxitem;
                if (taxitem.taxsplit == null || taxitem.taxsplit.length == 0) {
                    var taxpercentage = taxitem.taxpercentage;
                    var taxppart = taxpercentage > 0 ? taxpercentage / 2 : 0;
                    taxitem.taxsplit = {
                        'items': [{
                            'igst': taxppart, 'sourceid': 0, 'sgst': taxppart, 'toamount': 10000000,
                            'cgst': taxppart, 'taxpercent': taxpercentage, 'fromamount': 0
                        }]
                    };
                }
                taxitem.taxsplit.findTax = $scope.findTax;
                if ($scope.SupportMarginexcludetax && $scope.item.taxitem != null) {
                    $scope.item.price = 0;
                    $scope.item.discountpercentage = 0;
                    $scope.item.discount = 0;
                    $scope.item.dealerprice = 0;
                    $scope.item.displaymrp = 0;
                }
            }
        }

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

        $scope.JumpList = [];
        $scope.ShowSizeList = function () {
            var sizegroup = $scope.SizeGroups.find(function (item) {
                return item.id == $scope.item.sizegroupid;
            });
            if (typeof (sizegroup) != 'undefined' && sizegroup != null) {
                var sizelist = [];
                $.each(sizegroup.list, function (k, v) {
                    sizelist.push({ 'id': v.id, 'name': v.name, 'barcode': $scope.item.barcode, 'qty': '' });
                });
                $scope.item.sizegroup = sizelist;
                $scope.setFocusallqty();
            }
        }

        $scope.groupqtyChanged = function () {
            $.each($scope.item.sizegroup, function (k, v) {
                v.qty = $scope.item.groupqty;
            });
        }

        $scope.Save = function (moveNext) {
            if ($scope.SearchResult.length == 0)
                return;

            var loc = $scope.Location.find(function (item) {
                return item.id == $scope.SearchResult[0].locationid;
            });

            var request = {
                "Items": [],
                "SecurityToken": "String content"
            }

            $.each($scope.SearchResult, function (k, v) {
                if (typeof (v.stockid) == 'undefined' || v.stockid == 0) {

                    if (typeof (loc) != 'undefined' && loc != null && loc.additionalinfo != null)
                        v.iswarehouse = loc.additionalinfo.iswarehouse;
                    else
                        v.iswarehouse = false;

                    var duplicateitem = request.Items.find(function (item) {
                        return item.barcode == v.barcode;
                    })

                    if (typeof (duplicateitem) != 'undefined' && duplicateitem != null) {
                        duplicateitem.qty += v.qty;
                    }
                    else
                        request.Items.push(v);
                }
            });

            $scope.SaveRequest = true;
            var srequest = JSON.stringify(request);

            $http({
                method: 'POST',
                url: "StockService/MissingStock",
                data: srequest
            }).success(function (result) {
                $scope.SaveRequest = false;
                if (result != null && result.Success) {
                    var requestitems = $scope.SearchResult.length;
                    $scope.SearchResult = result.Items;
                    if (result.Items.length > 0 && requestitems != result.Items.length) {
                        $rootScope.Toaster.Info("Saved Successfully.!");
                        $rootScope.Toaster.Info('' + result.Items.length + ' new stock items created.');
                    }
                    else {
                        $rootScope.Toaster.Info("Saved Successfully.!");
                        $scope.status = 'Saved Successfully.!';
                    }
                }
                else {
                    $rootScope.Toaster.Error(result.ErrorMessage);
                }
            }).error(function (result) {
                $scope.SaveRequest = false;
            });
        }

        $scope.fileuploadedcomplete = function (value) {
            $scope.stockadjustmentexcel = null;
            var csupplier = -1;
            var cbarcode = -1;
            var cbrand = -1;
            var cproduct = -1;
            var cbuyingprice = -1;
            var csize = -1;
            var cprice = -1;
            var cqty = -1;
            var cisbit = -1;
            var cdiscount = -1;
            var cdiscountpercentage = -1;
            var chsn = -1;
            var cdesign = -1;

            if ($scope.item.companyid == 'undefined' || $scope.item.companyid == null || $scope.item.companyid == 0 ||
                $scope.item.locationid == 'undefined' || $scope.item.locationid == null || $scope.item.locationid == 0) {
                $rootScope.Toaster.Error("Please Select Company and Location");
                return;
            }
            $.each(value.columns, function (k, v) {

                switch (v.toLowerCase()) {
                    case 'barcode':
                        cbarcode = k;
                        break;
                    case 'suppliername':
                        csupplier = k;
                        break;
                    case 'product':
                        cproduct = k;
                        break;
                    case 'buyingprice':
                        cbuyingprice = k;
                        break;
                    case 'design':
                        cdesign = k;
                        break;
                    case 'brand':
                        cbrand = k;
                        break;
                    case 'price':
                        cprice = k;
                        break;
                    case 'qty':
                        cqty = k;
                        break;
                    case 'size':
                        csize = k;
                        break;
                    case 'bit':
                        cisbit = k;
                        break;
                    case 'discount':
                        cdiscount = k;
                        break;
                    case 'discountpercentage':
                        cdiscountpercentage = k;
                        break;
                    case 'hsncode':
                        chsn = k;
                        break;
                }

            });

            var errors = [];
            if (cbarcode == -1) {
                errors.push('barcode column missing');
            }
            if (csupplier == -1) {
                errors.push('supplier column missing');
            }
            if (cproduct == -1) {
                errors.push('product column missing');
            }
            if (cbuyingprice == -1) {
                errors.push('buying price column missing');
            }
            if (cprice == -1) {
                errors.push('price column missing');
            }
            if (cqty == -1) {
                errors.push('qty column missing');
            }
            if (cisbit == -1) {
                errors.push('bit column missing');
            }
            if (cdiscount == -1) {
                errors.push('discount column missing');
            }
            //if (cdiscountpercentage == -1) {
            //    errors.push('discountpercentage column missing');
            //}
            if (chsn == -1) {
                errors.push('hsncode column missing');
            }
            if (cdesign == -1) {
                errors.push('design column missing');
            }
            if (errors.length == 0)
                errors.push('No Conflict');

            errors.push('Do you want to continue?');

            $rootScope.Confirm('Import', errors.join('<br>'), function () {

                var missingrows = [];
                var skipped = 0;
                $.each(value.rows, function (k, v) {

                    var item = {
                        'productid': -1,
                        'companyid': $rootScope.GetNumber($scope.item.companyid),
                        'supplierid': $rootScope.GetNumber($scope.item.supplierid),
                        'locationid': $rootScope.GetNumber($scope.item.locationid)
                    };
                    if (cbarcode >= 0)
                        item.barcode = v.values[cbarcode];

                    if (item.barcode != 'barcode' && item.barcode != 'BARCODE') {
                        if (cprice >= 0)
                            item.price = v.values[cprice];
                        if (cbuyingprice >= 0)
                            item.buyingprice = v.values[cbuyingprice];
                        if (cproduct >= 0) {
                            var pname = v.values[cproduct];
                            var product = $scope.Products.find(function (item) {
                                return item.name == pname;
                            })
                            if (typeof (product) != 'undefined' && product != null) {
                                item.productid = product.id;
                            }
                        }
                        if (csupplier >= 0) {
                            var supname = v.values[csupplier];
                            var supplier = $scope.Suppliers.find(function (item) {
                                return item.name == supname;
                            })
                            if (typeof (supplier) != 'undefined' && supplier != null) {
                                item.supplierid = supplier.id;
                            }
                        }
                        if (cbrand >= 0) {
                            item.brandname = v.values[cbrand];
                            var product = $scope.Brands.find(function (item) {
                                return item.name == item.brandname;
                            })
                            if (typeof (product) != 'undefined' && product != null) {
                                item.brandid = product.id;
                            }
                        }
                        if (csize >= 0) {
                            item.sizename = v.values[csize];
                            var product = $scope.Sizes.find(function (item) {
                                return item.name == item.sizename;
                            })
                            if (typeof (product) != 'undefined' && product != null)
                                item.sizeid = product.id;
                        }
                        if (cqty >= 0)
                            item.qty = $rootScope.GetNumber(v.values[cqty]).round(2);

                        if (cdiscount >= 0)
                            item.discount = $rootScope.GetNumber(v.values[cdiscount]).round(2);

                        if (cdiscountpercentage >= 0)
                            item.discountpercentage = $rootScope.GetNumber(v.values[cdiscountpercentage]).round(2);
                        else {
                            if (cdiscount == 0)
                                item.discountpercentage = 0;
                            else
                                item.discountpercentage = ((item.discount / item.qty) / item.price * 100).round(0);
                        }
                        if (chsn >= 0 && (typeof (item.hsncode) == 'undefined' || item.hsncode == null))
                            item.hsncode = v.values[chsn];
                        if (cisbit >= 0)
                            item.isbit = v.values[cisbit].toLowerCase() == "yes";
                        if (cdesign >= 0)
                            item.designid = v.values[cdesign];
                        if (item.productid >= 0) {
                            item.sellingname = $scope.SearchRef(item.productid, $scope.Products);
                            //item.locationid = $rootScope.LocationInfo.location.id;
                            item.directstockupload = true;
                            $scope.SearchResult.push(item);
                        }
                        else {
                            skipped++;
                            if (missingrows.indexOf(v.values[cproduct]) < 0)
                                missingrows.push(v.values[cproduct]);
                        }
                    }
                });

                if (missingrows.length > 0) {
                    $rootScope.Confirm('Missing Products - ' + missingrows.length, missingrows.join('<br>') + '<br>Skipped Rows - ' + skipped, function () {
                        $scope.$$phase || $scope.$digest();
                    });
                }
                else {
                    $scope.igrid.UpdateData($scope.SearchResult);

                    $timeout(function () {
                        $scope.$$phase || $scope.$digest();
                    }, 10);
                }
            });
        }

        $scope.searchserial = function () {
            restService.Get('SalablegoodsService/UAN/' + $rootScope.LocationInfo.location.id + '/' + $scope.item.serialno).then(function (results) {
                $scope.item.stock = (results.iqty - (results.dqty + results.oqty));
                $scope.item.hsncode = results.hsncode;
                $scope.item.price = results.price;
                $scope.item.barcode = results.barcode;
                $scope.item.designid = results.designid;
                $scope.item.serialno = results.serialno;
                $scope.item.companyid = results.companyid;
                $scope.item.discount = results.discount;
                $scope.item.sizeid = results.sizeid;
                $scope.item.colourid = results.colourid;
                $scope.item.styleid = results.styleid;
                $scope.item.patternid = results.patternid;
                $scope.item.fitid = results.fitid;
                $scope.item.materialid = results.materialid;
                $scope.item.productid = results.productid;
                $scope.item.discountpercentage = (results.discount / results.price * 100).round(0);
                $scope.item.stockid = results.stockid;
                $scope.item.itemid = results.itemid;
                $scope['setFocuscompanyid']();
            });
        }

        $scope.searchbarcode = function () {
            restService.Get('SalablegoodsService/ReturnBarcode/' + $rootScope.LocationInfo.location.id + '/' + $scope.item.barcode).then(function (results) {
                $scope.item.stock = (results.iqty - (results.dqty + results.oqty));
                $scope.item.hsncode = results.hsncode;
                $scope.item.price = results.price;
                $scope.item.serialno = results.serialno;
                $scope.item.discount = results.discount;
                $scope.item.productid = results.productid;
                $scope.item.discountpercentage = (results.discount / results.price * 100).round(0);
                $scope.item.stockid = results.stockid;
                if (results.id > 0)
                    $scope['setFocusqty']();
                else
                    $scope['setFocusuancode']();
            });
        };

        $scope.DiscountChanged = function (type) {
            if (type == 'Value') {
                $scope.item.discountpercentage = ($scope.item.discount / $scope.item.price * 100).round(2);
            }
            else {
                $scope.item.discount = ($scope.item.price * $scope.item.discountpercentage / 100).round(2);
            }
        }

        $scope.SearchCondition = {
            companyid: "",
            productid: "",
            supplierid: "",
            barcode: ""
        }


        $scope.ItemHistory = function () {

            request = {
                "Item": {
                    barcode: $scope.barcode,
                },
                "securitytoken": "String content"
            }

            var srequest = JSON.stringify(request);

            $http({
                method: 'POST',
                url: "StockService/ItemHistory",
                data: srequest
            }).success(function (result) {

                $scope.SearchRetailResult = result;
            }).error(function (result) {
            });

        }


        $scope.PrintBarcode = function () {
            var ids = [];
            var iswarehouse = false;
            var locationid = 0;
            var _selectindex = 1;
            $.each(this.igrid.GetSelectedRows(), function (k, v) {
                ids.push({
                    'barcode': v.barcode,
                    'noofcopy': v.qty,
                    'selectindex': _selectindex++
                });
                iswarehouse = v.iswarehouse;
                locationid = v.locationid;
            });

            $scope.ChoseReport({ 'iswarehouse': iswarehouse, 'locationid': locationid, 'barcodes': ids, 'reportid': 0, 'ismfg': false });
        }

        $scope.PrintUAN = function () {
            var ids = [];
            var iswarehouse = false;
            var locationid = 0;
            $.each(this.igrid.GetSelectedRows(), function (k, v) {
                ids.push({
                    'uid': v.barcode,
                    'pqty': v.qty
                });
                iswarehouse = v.iswarehouse;
                locationid = v.locationid;
            });

            $scope.ChoseMfgReport({ 'iswarehouse': iswarehouse, 'locationid': locationid, 'barcodes': ids, 'reportid': 0, 'ismfg': true });
        };

        $scope.ChoseMfgReport = function (requestitem) {
            $rootScope.Handlers.SelectPrinter('BARCODE', function (printdata) {
                if (printdata == null || printdata.report == null || printdata.report.id <= 0)
                    return;

                requestitem.reportid = printdata.report.id;
                requestitem.list = requestitem.barcodes;
                var request = {
                    "Item": requestitem,
                    "SecurityToken": "String content"
                }
                var srequest = JSON.stringify(request);
                $http({
                    method: 'POST',
                    url: "MfgdesignService/PrintBarcode",
                    data: srequest
                }).success(function (result) {

                    if (result != null && result.length > 0) {
                        $scope.Sent2Printer(result, printdata);
                    }
                });
            });
        }

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

        $scope.DefineGrid = function () {
            if ($scope.igrid != null)
                $scope.igrid.destroy();

            var igrid = new InfyGrid('gridContainer', '450px', '100%', true, true, true, 0);
            igrid.options.showHeaderRow = true;
            igrid.AddSimpleColumn('sno', 'S.No', 50);
            igrid.AddReferenceColumn('locationid', 'Location', 150, $scope.Location);
            igrid.AddSimpleColumn('barcode', 'Barcode', 120);
            if (!$scope.system_ismfgapplication) {
                igrid.AddSimpleColumn('serialno', 'UAN', 120);
            }
            igrid.AddSimpleColumn('sellingname', 'Product', 150);
            igrid.AddSimpleColumn('brandname', 'Brand', 80);
            igrid.AddSimpleColumn('sizename', 'Size', 80);
            igrid.AddSimpleColumn('designid', 'Design', 80);
            igrid.AddSimpleColumn('hsncode', 'Hsncode', 80);
            igrid.AddFormatedColumn('qty', 'Qty', 70, 'N2', 2);
            igrid.AddFormatedColumn('buyingprice', 'Cost', 70, 'N2', 2);
            igrid.AddFormatedColumn('price', 'Price', 70, 'N2', 2);
            igrid.AddFormatedColumn('discountpercentage', 'Discnt%', 70, 'N2', 2);
            igrid.AddFormatedColumn('discount', 'Discount', 70, 'N2', 2);
            if ($scope.SupportDealerPrice) {
                igrid.AddFormatedColumn('dealerprice', 'Dealer Price', 80, 'N2', 2);
                igrid.AddFormatedColumn('displaymrp', 'MRP', 60, 'N2', 2);
            }

            var column = igrid.AddCustomColumn('sellingprice', 'Final', 70, function (item, col) {
                return $filter('IndianNumber')(item.price - item.discount, 2);
            });
            column.cssClass = 'rightalign';
            column.total = true;
            column.getValue = function (item, col) {
                return item.price - item.discount;
            }

            igrid.options.autoEdit = true;
            igrid.Initialize();
            $scope.igrid = igrid;
            igrid.onSelectedRangesChanged = function (range, rowmodel) {
                $timeout(function () {
                    $scope.selectedcount = range.length;
                }, 10);
            };
            $scope.igrid.UpdateData($scope.SearchResult);
        }

        $scope.exporttoExcel = function (infygrid, filename, tableid) {
            var title = $rootScope.UserInfo.CompanyName;
            var subtitle = 'DIRECT STOCK ENTRY'
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

        $scope.Sent2Printer = function (printdocument, printerData) {
            $rootScope.Sent2Printer(printdocument.join(';'), printerData);
            return;

            $.each(printdocument, function (k, v) {

                $rootScope.Sent2Printer(v, printerData);

                //var request = {
                //    'id': printerData.printerid,
                //    'content': v,
                //    'encoded': 'BASE64'
                //};
                //var srequest = JSON.stringify(request);
                //$http({
                //    method: 'POST',
                //    url: printerData.service,
                //    data: srequest
                //}).success(function (result) {

                //}).error(function (error) {

                //});
            })
        }
        $scope.ConfigureGrid = function () {
            if (typeof ($scope.Location) == 'undefined' || $scope.Location == null || $scope.Location.length == 0) {
                $timeout($scope.ConfigureGrid(), 100);
            }
            else
                $scope.DefineGrid();
        };

        $timeout(function () {
            $scope.ConfigureGrid();
        }, 100);


    }]);
