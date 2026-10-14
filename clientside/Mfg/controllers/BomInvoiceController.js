var app = angular.module('InfinityLab');

app.controller('BomInvoiceController', ['UserDetails', '$scope', '$http', '$location', '$rootScope', '$window', 'restService', '$timeout', 'ngDialog', 'cntxService',
    function (UserDetails, $scope, $http, $location, $rootScope, $window, restService, $timeout, ngDialog, cntxService) {
        $scope.Navigation = $rootScope.GetNavigation();
        $scope.Navigation.SetMode('EDIT');
        $scope.LocalContext = cntxService.CreatePageContext('lrinvoiceitems');
        $scope.ProductSearch = { "ProductID": null, "SearchItemName": "", "currentInvoiceID": "" };
        $scope.InvoiceInfo = { invoiceid: $location.search().invoiceid };
        $scope.AutoCalculation = false;
        $scope.SearchRef = restService.SearchReference;
        $scope.ShowSearch = true;
        $scope.showtax = false;
        $scope.SaveRequest = false;
        $scope.SearchResult = [];
        $scope.ErrorMessage = '';
        $scope.InvoiceTaxItem = {};
        $scope.CurrentPOList = [];
        $scope.additionaldiscountpercentage = 0;
        $scope.Total = {
            "buyingqty": 0,
            "mrp": 0,
            "sellingprice": 0,
            "buyingprice": 0,
            "tax": 0,
            "buyingpiece": 0
        };

        $scope.SwitchPanel = function () {
            $scope.ShowSearch = !$scope.ShowSearch;
        }
        $scope.ShowTax = function () {
            $scope.showtax = !$scope.showtax;
        }

        //$scope.ItemDetails.poinfo = [];
        //$scope.ItemDetails.polist = '';
        $scope.SearchCondition = {
            companyid: 0,
            supplierid: 0,
            invoiceno: "",
            addedfrom: null,
            addedto: null,
            lrentryno: ""
        }

        $scope.PurchaseOrderItems = [];

        $scope.WarehouseLocations = restService.Get('ReferencelistService/WHLOCATION', true).then(function (results) {
            $scope.WarehouseLocations = results;
        });

        $scope.Transport = restService.Get('ReferencelistService/TRANSPORT', true).then(function (results) {
            $scope.Transport = results;
        });

        $scope.PurchaseTax = restService.Get('ReferencelistService/PURCHASETAX', true).then(function (results) {
            $scope.purchaseTaxActual = results;
            var taxlist = [];
            $.each(results, function (k, v) {
                if (typeof (v.taxsplit) == 'undefined' || typeof (v.taxsplit.items) == 'undefined' || v.taxsplit.items == null) {
                    var taxitem = (JSON.parse(JSON.stringify(v)));
                    taxitem.name = 'GST @ ' + taxitem.taxpercentage + '%';
                    taxitem.code = '' + taxitem.id + '_' + 1;
                    taxitem.taxsplit = { 'items': [{ 'cgst': taxitem.taxpercentage, 'sgst': 0 }] };
                    taxitem.type = 0;
                    taxlist.push(taxitem);
                }
                else {
                    $.each(v.taxsplit.items, function (kk, vv) {
                        var taxitem = (JSON.parse(JSON.stringify(v)));
                        taxitem.taxpercentage = vv.cgst + vv.sgst;
                        taxitem.type = 0;
                        if (v.taxsplit.items.length > 1)
                            taxitem.name = 'GST @ ' + taxitem.taxpercentage + '% (' + vv.fromamount + ' - ' + vv.toamount + ')';
                        else
                            taxitem.name = 'GST @ ' + taxitem.taxpercentage + '%';
                        taxitem.code = '' + taxitem.id + '_' + kk;
                        taxlist.push(taxitem);
                    });
                }
            })
            $scope.PurchaseTax = taxlist;

            $scope.PurchaseTax.push({
                'id': -1, 'name': 'Rounding', 'taxpercentage': 'Rounding', 'type': 9
            });


            $scope.AdditionalChargeList = restService.Get('ReferencelistService/ADDITIONALCHARGES', true).then(function (results) {
                $.each(results, function (k, v) {

                    var taxitem = $scope.purchaseTaxActual.find(function (item) {
                        return item.id == v.referenceid;
                    });
                    if (typeof (taxitem) != 'undefined' && taxitem != null) {
                        var taxitem = (JSON.parse(JSON.stringify(taxitem)));
                        taxitem.name = v.name + ' @ ' + taxitem.taxpercentage + '%';
                        taxitem.code = 'Ad_' + taxitem.id + '_' + 1;
                        if (taxitem.taxpercentage > 0)
                            taxitem.taxsplit = { 'items': [{ 'cgst': taxitem.taxpercentage / 2, 'sgst': taxitem.taxpercentage / 2 }] };
                        else
                            taxitem.taxsplit = { 'items': [{ 'cgst': 0, 'sgst': 0 }] };
                        taxitem.type = 8;
                        $scope.PurchaseTax.push(taxitem);
                    }
                });
            });

            $scope.ColorList = [];
            $scope.AllColorList = [];
            restService.Get('ReferencelistService/COLOUR', true).then(function (results) {
                $scope.ColorList = results;
                $scope.AllColorList = results;
            });

            $scope.AdditionalChargeList = restService.Get('ReferencelistService/ADDITIONALCHARGES', true).then(function (results) {
                $.each(results, function (k, v) {

                    var taxitem = $scope.purchaseTaxActual.find(function (item) {
                        return item.id == v.referenceid;
                    });

                    var taxitem = (JSON.parse(JSON.stringify(taxitem)));
                    taxitem.name = v.name + ' @ ' + taxitem.taxpercentage + '%';
                    taxitem.code = 'Ad_' + taxitem.id + '_' + 1;
                    if (taxitem.taxpercentage > 0)
                        taxitem.taxsplit = { 'items': [{ 'cgst': taxitem.taxpercentage / 2, 'sgst': taxitem.taxpercentage / 2 }] };
                    else
                        taxitem.taxsplit = { 'items': [{ 'cgst': 0, 'sgst': 0 }] };
                    taxitem.type = 8;
                    $scope.PurchaseTax.push(taxitem);
                });

            });
        });

        $scope.BuyingCompany = restService.Get('ReferencelistService/BUYINGCOMPANY', true).then(function (results) {
            $scope.BuyingCompany = results;
        });

        $scope.SupplierCompany = restService.Get('ReferencelistService/SUPPLIERCOMPANY', true).then(function (results) {
            $scope.SupplierCompany = results;
        });
            
        $scope.Suppliers = restService.Get('ReferencelistService/SUPPLIER', true).then(function (results) {
            $scope.Suppliers = results;
        });

        $scope.BOMAllList = restService.Get('ReferencelistService/BOMLIST', true).then(function (results) {
            $scope.BOMAllList = results;
        });
        $scope.BOMCategories = restService.Get('ReferencelistService/FULL-BOMCATEGORY', true).then(function (results) {
            $scope.BOMCategories = results;
        });


        $scope.StatusList = [{ id: 1, name: 'Invoice Progress' }, { id: 3, name: 'Temporary' }, { id: 2, name: 'Invoice Completed' }];
        $scope.InvoiceType = [{ id: 0, name: 'GRN Entry' }, { id: 1, name: 'DC Entry' }, { id: 2, name: 'GRN for DC' }];

        $scope.ItemTemplate = {
            "buyingqty": null,
            "mrp": null,
            "sellingprice": null,
            "buyingprice": null,
            "tax": 0,
            "staxpercentage": 0,
            "ptaxpercentage": 0,
            "taxid": null,
            "pricingmode": 0,
            "calculatedPrice": 0,
            "readonly": false,
            "isactive": true,
            "designid": "",
            "sno": 1,
            Details: {
                "productid": null,
                "printingname": null,
                "brandid": null,
                "patternid": null,
                "styleid": null,
                "typeid": null,
                "sizeid": null,
                "colourid": null,
                "materialid": null
            }
        }

        $scope.InvoiceTemplate = {
            "companyid": 2, "invoiceno": null, "invoicedate": "", "invoiceDate": "", "noofpieces": 0, "noofbundle": 0, "baseamount": 0.0, "itemid": 0,
            "grossamount": 0.0, "totalamount": 0.0, "lrid": 0, "id": 0, "otheramount": 0.0, "modifiedon": "", "createdon": "","invoicetypeid":0,
            "createdby": 0, "modifiedby": 0, "version": 1, "suppliercompanyid": 0, "isactive": true, bominvoicetaxitems: [], uniqueid: 0,
            bominvoiceitems: [], Lrentrydetails: []
        }

        $scope.LastPrice = null;

        $scope.ItemDetails = (JSON.parse(JSON.stringify($scope.ItemTemplate)));
        $scope.Invoice = (JSON.parse(JSON.stringify($scope.InvoiceTemplate)));
        //$scope.AddTax = (JSON.parse(JSON.stringify($scope.AdditionalTax)));
        $scope.Invoice.bominvoicetaxitems = [];
        $scope.Invoice.Lrentrydetails = [];


        $scope.categoryChange = function () {
            if (typeof ($scope.ItemDetails.bomcategoryid) != 'undefined' && $scope.ItemDetails.bomcategoryid != null) {
                var BOMList = [];
                $.each($scope.BOMAllList, function (k, v) {
                    if (v.categoryid == $scope.ItemDetails.bomcategoryid)
                        BOMList.push(v);
                });
                $scope.BOMList = BOMList;
            }
        }

        $scope.colourChange = function () {
            $scope.UpdatePOList();
        }

        $scope.productChange = function () {
            if ($scope.ItemDetails.bomid && $scope.ItemDetails.bomid != null) {
                var prditem = $scope.BOMList.find(function (item) {
                    return (item.id == $scope.ItemDetails.bomid);
                });
                $scope.ItemDetails.fgproductid = 0;
                $scope.ItemDetails.fgskuid = '';
                if (prditem || prditem != null) {
                    $scope.UpdatePOList();
                    $scope.ItemDetails.taxid = prditem.taxid;
                    if (prditem.coretypeid && prditem.coretypeid == 3) {
                        $scope.ItemDetails.fgproductid = prditem.tproductid;
                        $scope.ItemDetails.fgskuid = prditem.tskuid;
                    }
                    $scope.taxChange();
                    $scope.SetColourList(prditem);
                }
            };
            $scope.fixPrice();
        };

        $scope.setColourFocus = function () {
            if ($scope.ColorList.length > 0)
                $scope.setFocuscolourid();
            else
                $scope.setFocuswidth();
        }

        $scope.SetColourList = function (prditem) {
            $scope.ItemDetails.colourid = null;
            $scope.ItemDetails.bominwardcode = '';
            if (prditem.colourmode == 2) {
                $scope.ColorList = [];
                $.each(prditem.colourlist, function (k, v) {
                    var colour = $scope.AllColorList.find(function (item) {
                        return item.id == v.id;
                    });
                    if (colour && colour.id > 0) {
                        colour.bominwardcode = v.name;
                        colour.imageid = v.imageid;
                        $scope.ColorList.push(colour);
                    }
                });
            } else if (prditem.colourmode == 1) {
                $scope.ColorList = [];
                var colour = $scope.AllColorList.find(function (item) {
                    return item.id == prditem.colorid;
                });
                if (colour && colour.id > 0) {
                    colour.bominwardcode = v.name;
                    colour.imageid = v.imageid;
                    $scope.ColorList.push(colour);
                }
            }
            else {
                $scope.ColorList = [];
                $scope.ItemDetails.bominwardcode = prditem.code;
                $scope.ItemDetails.imageid = prditem.imageid;
            }
        }

        $scope.CurrentTaxConfig = null;
        $scope.taxChange = function () {
            $scope.ItemDetails.ptaxpercentage = 0;
            $scope.ItemDetails.staxpercentage = 0;
            if ($scope.ItemDetails.taxid != null) {
                var taxitem = $scope.purchaseTaxActual.find(function (item) {
                    return (item.id == $scope.ItemDetails.taxid);
                });
                if (taxitem || taxitem != null) {
                    $scope.CurrentTaxConfig = taxitem;
                    if (taxitem != null && typeof (taxitem.taxsplit.findTax))
                        taxitem.taxsplit.findTax = $scope.findTax;
                    $scope.fixTax();
                }
            };
            $scope.fixPurhcaseDiscount();
        };

        $scope.fixTax = function (findDiscount) {
            var taxitem = $scope.CurrentTaxConfig;
            if (taxitem == null && $scope.ItemDetails.taxid > 0) {
                $scope.taxChange();
                taxitem = $scope.CurrentTaxConfig;
            }
            if (typeof (findDiscount) == 'undefined' || findDiscount) {
                $scope.fixPurhcaseDiscount();
                if (taxitem == null) {
                    $scope.ItemDetails.purchasetaxpercentage = 0;
                    $scope.ItemDetails.salestaxpercentage = 0;
                    $scope.ItemDetails.salestax = 0;
                    $scope.ItemDetails.purchasetax = 0;
                    $scope.fixPrice();
                    return;
                }
            }

            $scope.fixPrice();
            var taxconfig = taxitem.taxsplit.findTax($scope.ItemDetails.buyingprice);
            if (taxconfig.cgst == 0) {
                taxconfig = { 'cgst': 0, 'sgst': 0 };
            }
            $scope.ItemDetails.purchasetaxpercentage = taxconfig.cgst + taxconfig.sgst;
            $scope.calculatesalestax();
            $scope.marginChanged();
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

        $scope.fixPurhcaseDiscount = function () {

            var discountvalue = 0;
            var grossvalue = 0;
            var discountpercent = 0;
            if (typeof ($scope.Invoice.bominvoicetaxitems) == 'undefined' || $scope.Invoice.bominvoicetaxitems == null)
                $scope.Invoice.bominvoicetaxitems = [];

            /*var discountitem = $scope.Invoice.bominvoicetaxitems.find(function (k) {
                return Math.abs(k.taxid) == Math.abs($scope.ItemDetails.taxid);
            })*/

            $.each($scope.Invoice.bominvoicetaxitems, function (k, discountitem) {
                if ($scope.ItemDetails.taxid == discountitem.taxid && discountitem.type == 0) {
                    discountvalue += discountitem.discount;
                    grossvalue += discountitem.amounton;
                }
            });

            if (grossvalue > 0 && discountvalue > 0) {
                discountpercent = (discountvalue / grossvalue * 100).round(2);
            }
            //$scope.ItemDetails.purchasediscountpercentage = (discountpercent + $scope.additionaldiscountpercentage).round(2);
        }

        $scope.fixPrice = function () {

            $scope.ItemDetails.purchasediscount = ($scope.GetNumber($scope.ItemDetails.basebuyingprice) * $scope.ItemDetails.purchasediscountpercentage / 100).round(5);
            if (isNaN($scope.ItemDetails.purchasediscount))
                $scope.ItemDetails.purchasediscount = 0;

            $scope.ItemDetails.buyingprice = ($scope.GetNumber($scope.ItemDetails.basebuyingprice) - $scope.ItemDetails.purchasediscount).round(2);

        };

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


        $scope.taxablevaluechanged = function () {
            var taxitem = $scope.PurchaseTax.find(function (item) {
                return item.code == $scope.InvoiceTaxItem.taxcode;
            });
            if (typeof (taxitem) != 'undefined' && taxitem != null) {
                $scope.InvoiceTaxItem.taxid = taxitem.id;
                $scope.InvoiceTaxItem.taxpercentage = taxitem.taxpercentage;
                var taxable = $scope.GetNumber($scope.InvoiceTaxItem.taxablevalue) - $scope.GetNumber($scope.InvoiceTaxItem.discount);
                $scope.InvoiceTaxItem.taxamount = (taxable * $scope.GetNumber(taxitem.taxpercentage) / 100).round(2);
            }
        }

        $scope.CheckTaxValue = function (event) {
            if (typeof ($scope.InvoiceTaxItem.taxablevalue) == 'undefined'
                || $scope.InvoiceTaxItem.taxablevalue == null
                || $scope.InvoiceTaxItem.taxablevalue == '') {

                event.preventDefault();
                event.stopPropagation();
                $scope.setFocussno();
                return;
            }
        }

        $scope.AddTaxItem = function () {

            if (typeof ($scope.InvoiceTaxItem.discount) != 'undefined' &&
                $scope.InvoiceTaxItem.discount != null &&
                $scope.InvoiceTaxItem.discount.toString().indexOf('%') > 0) {

                var discountondiscount = 0;
                var discountpercent = 0;

                if ($scope.InvoiceTaxItem.discount.toString().indexOf('+') > 0) {
                    var discountparts = $scope.InvoiceTaxItem.discount.toString().split('+');
                    $scope.InvoiceTaxItem.discount = discountparts[0];
                    discountondiscount = $scope.GetNumber(discountparts[1].toString().replace('%', ''));
                }

                discountpercent = $scope.GetNumber($scope.InvoiceTaxItem.discount.toString().replace('%', ''));

                $scope.purchasediscountpercentage = discountpercent;
                $scope.InvoiceTaxItem.purchasediscountpercentage = discountpercent;

                if ($scope.Invoice.isinclusivediscount) {
                    $scope.InvoiceTaxItem.taxablevalue = ($scope.GetNumber($scope.InvoiceTaxItem.taxablevalue) / (100 - discountpercent) * 100).round(2);
                }

                $scope.InvoiceTaxItem.discount = ($scope.GetNumber($scope.InvoiceTaxItem.taxablevalue) * discountpercent / 100).round(2);

                if (discountondiscount > 0) {
                    var discountondiscountvalue = (($scope.GetNumber($scope.InvoiceTaxItem.taxablevalue) - $scope.InvoiceTaxItem.discount) * discountondiscount / 100).round(2);
                    $scope.purchasediscountpercentage = ($scope.purchasediscountpercentage + (discountondiscountvalue / $scope.InvoiceTaxItem.taxablevalue * 100)).round(2);
                    $scope.InvoiceTaxItem.purchasediscountpercentage = $scope.purchasediscountpercentage;
                    $scope.InvoiceTaxItem.discount = ($scope.InvoiceTaxItem.discount + discountondiscountvalue).round(2);
                }

                $scope.taxablevaluechanged(false);
                return;
            }

            $scope.taxablevaluechanged(false);

            if (typeof ($scope.Invoice.bominvoicetaxitems) == 'undefined' || $scope.Invoice.bominvoicetaxitems == null)
                $scope.Invoice.bominvoicetaxitems = [];

            var taxitem = $scope.PurchaseTax.find(function (item) {
                return item.code == $scope.InvoiceTaxItem.taxcode;
            });

            var lrtaxitem = {
                'taxid': taxitem.id,
                'type': taxitem.type,
                'percentage': taxitem.taxpercentage,
                'amounton': $scope.GetNumber($scope.InvoiceTaxItem.taxablevalue),
                'discount': $scope.InvoiceTaxItem.discount,
                'taxamount': $scope.InvoiceTaxItem.taxamount,
                'isactive': true,
                'netamount': $scope.GetNumber($scope.InvoiceTaxItem.taxablevalue) +
                    $scope.GetNumber($scope.InvoiceTaxItem.taxamount) - $scope.GetNumber($scope.InvoiceTaxItem.discount)
            };
            $scope.Invoice.bominvoicetaxitems.push(lrtaxitem);
            $scope.ComputeTaxTotal();
            $scope.InvoiceTaxItem.taxablevalue = null;
            $scope.InvoiceTaxItem.discount = null;
            $scope.InvoiceTaxItem.taxamount = null;
            $scope.setFocustaxid();
        }

        $scope.ComputeTaxTotal = function () {
            var netamount = 0; var netamounton = 0; var nettax = 0;
            var netdiscount = 0; var netdiscountpercent = 0;
            var othercharge = 0; var othertax = 0; var otherdiscount = 0;
            $.each($scope.Invoice.bominvoicetaxitems, function (k, v) {
                if (v.type == 8) {
                    othertax += v.taxamount;
                    otherdiscount += $scope.GetNumber(v.discount);
                    othercharge += v.amounton;
                }
                else {
                    netamount += v.netamount;
                    netamounton += v.amounton;
                    nettax += v.taxamount;
                    netdiscount += $scope.GetNumber(v.discount);
                }
                if ($scope.GetNumber(v.discount) > 0) {
                    v.discountpercentage = ($scope.GetNumber(v.discount) / v.amounton * 100).round(2);
                }
            });
            $scope.Invoice.otheramount = othercharge;
            $scope.Invoice.othertax = othertax;
            $scope.Invoice.billvalue = netamounton;
            $scope.Invoice.taxamount = nettax + othertax;
            $scope.Invoice.purchasediscount = netdiscount + otherdiscount;
            $scope.Invoice.purchasediscountpercentage = (netdiscount / netamounton * 100).round(2);
            $scope.Invoice.totalamount = (netamount + othertax - otherdiscount + $scope.GetNumber($scope.Invoice.otheramount)).round(2);
            //$scope.Invoice.payableamount = $scope.Invoice.totalamount - $scope.GetNumber($scope.Invoice.debitnotediscount);
        }

        $scope.imageuploaded = function (imageid, scope) {
            scope.item.imageid = null;
            scope.item.timageid = null;
            $rootScope.CropImage(imageid, 'IMAGE', function (data) {
                scope.item.imageid = data;
            }, null, { 'width': 200, 'height': 100 });
        }

        $scope.AddItem = function () {

            if (typeof ($scope.ItemDetails.bomid) == 'undefined' || $scope.ItemDetails.bomid == null || $scope.ItemDetails.bomid <= 0) {
                $rootScope.Toaster.Info("Please select the component");
                return false;
            }

            if (typeof ($scope.ItemDetails.bomcategoryid) == 'undefined' || $scope.ItemDetails.bomid == null || $scope.ItemDetails.bomcategoryid <= 0) {
                $rootScope.Toaster.Info("Please select the catagory");
                return false;
            }

            var duplicatesno = $scope.Invoice.bominvoiceitems.find(function (item) {
                return item.sno == $scope.ItemDetails.sno;
            });

            if (typeof (duplicatesno) != 'undefined' && duplicatesno != null) {
                $rootScope.ShowAlert('Please enter different serial number', 'Duplicate serial no');
                return false;
            };
            if ($scope.ItemDetails.colourid && $scope.ItemDetails.colourid > 0) {
                var colour = $scope.ColorList.find(function (item) {
                    return item.id == $scope.ItemDetails.colourid;
                });
                if (colour && colour.id > 0) {
                    $scope.ItemDetails.bominwardcode = colour.bominwardcode;
                    $scope.ItemDetails.imageid = colour.imageid;
                }
            }


            $scope.ItemDetails.bomname = $scope.SearchRef($scope.ItemDetails.bomid, $scope.BOMAllList);
            $scope.ItemDetails.bominwardcode = '';

            if ($scope.ValidateFGTradingItem()) {
                $scope.AddItemInternal(true);
                $scope.ItemDetails.buyingamount = ($scope.GetNumber($scope.ItemDetails.basebuyingprice) * $scope.GetNumber($scope.ItemDetails.buyingqty)).round(2);
                $scope.CreateNew();
                if ($scope.Invoice.bominvoiceitems.length == 1)
                    $scope.ConfigureEditGrid();
                else
                    $scope.egrid.UpdateData($scope.Invoice.bominvoiceitems);
                $scope.setFocussno();
                return true;
            }
            else {
                return false;
            }
        };

        $scope.ValidateFGTradingItem = function () {
            if ($scope.ItemDetails.fgskuid == undefined ||
                $scope.ItemDetails.fgproductid == undefined ||
                $scope.ItemDetails.fgskuid == null ||
                $scope.ItemDetails.fgproductid == null ||
                $scope.ItemDetails.fgskuid == null ||
                $scope.ItemDetails.fgproductid == null ||
                $scope.ItemDetails.fgskuid == '' ||
                $scope.ItemDetails.fgproductid == '' ||
                $scope.ItemDetails.fgproductid == 0)
                return true;

            var request = {
                "Item": {
                    'name': $scope.ItemDetails.fgskuid,
                    'productid': $scope.ItemDetails.fgproductid,
                    'colourid': $scope.ItemDetails.colourid
                },
                "SecurityToken": "String content"
            }

            $http({
                method: 'POST',
                url: "MfgdesignService/SearchUAN",
                data: request
            }).success(function (result) {
                if (result != null) {
                    $scope.ItemDetails.colourid = result.colourid;
                    $scope.ItemDetails.fguid = result.uid
                    $scope.AddItemInternal(true);
                    $scope.ItemDetails.buyingamount = ($scope.GetNumber($scope.ItemDetails.basebuyingprice) * $scope.GetNumber($scope.ItemDetails.buyingqty)).round(2);
                    $scope.CreateNew();

                    if ($scope.Invoice.bominvoiceitems.length == 1)
                        $scope.ConfigureEditGrid();
                    else
                        $scope.egrid.UpdateData($scope.Invoice.bominvoiceitems);
                    $scope.setFocussno();
                }
            }).error(function (result) {

            });
        }

        $scope.LastSaved = function () {
            var value = $scope.LocalContext.Get('$scope.BOMInvoice', false);
            if (typeof (value) != 'undefined' && value == null) {
                $rootScope.ShowAlert('No data found', 'No-Data');
                return;
            }
            $scope.Invoice = value;
            if ($scope.Invoice.bominvoiceitems.length == 1) {
                $scope.ConfigureEditGrid();
            }
            else {
                $scope.egrid.UpdateData($scope.Invoice.bominvoiceitems);
            }
        };

        $scope.New = function () {
            $scope.ItemDetails = (JSON.parse(JSON.stringify($scope.ItemTemplate)));
            $scope.Invoice = (JSON.parse(JSON.stringify($scope.InvoiceTemplate)));
            $scope.Invoice.bominvoicetaxitems = [];
            $scope.Invoice.Lrentrydetails = [];
            $scope.ConfigureEditGrid();
            $scope.egrid.UpdateData($scope.Invoice.bominvoiceitems);
        }

        $scope.CreateNew = function () {

            $scope.ItemDetails = (JSON.parse(JSON.stringify($scope.ItemDetails)));
            $scope.ItemDetails.buyingqty = null;
            $scope.ItemDetails.buyingprice = null;
            $scope.ItemDetails.basebuyingprice = null;
            $scope.ItemDetails.sellingprice = null;
            $scope.ItemDetails.mrp = null;
            $scope.ItemDetails.width = null;
            $scope.ItemDetails.purchasetax = null;
            $scope.ItemDetails.salestax = null;
            $scope.ItemDetails.purchasediscount = null;
            $scope.ItemDetails.purchasediscountpercentage = null;

            if ($scope.Invoice.bominvoiceitems.length > 0)
                $scope.ItemDetails.sno = parseInt($scope.Invoice.bominvoiceitems[$scope.Invoice.bominvoiceitems.length - 1].sno) + 1;
            else
                $scope.ItemDetails.sno = 1;

            $scope['setFocussno']();

            $scope.LocalContext.Post('$scope.BOMInvoice', $scope.Invoice, false);
        }

        $scope.defaultTaxID = function () {
            if ($scope.Invoice.bominvoicetaxitems.length > 0)
                return $scope.Invoice.bominvoicetaxitems[0].taxid;
            return null;
        };

        $scope.LoadInvoice = function () {
            $scope.InvoiceInfo.invoiceid = $scope.ProductSearch.currentInvoiceID;
            $scope.InitializeInvoiceList(true);
        };

        $scope.AddMultipleItemInternal = function (qty, pieces) {
            $scope.ItemDetails.productid = $scope.ProductSearch.ProductID;
            $scope.ItemDetails.Details.productid = $scope.ProductSearch.ProductID;
            $scope.ItemDetails.searchtext = $scope.ProductSearch.SearchItemName;
            $scope.ItemDetails.sno = parseFloat($scope.ItemDetails.sno);

            if (typeof ($scope.ItemDetails.itemid) == 'undefined' || $scope.ItemDetails.itemid == 0) {
                $scope.ItemDetails.Details.printingname = $scope.SearchRef($scope.ItemDetails.productid, $scope.ProductList, '');
            }

            if (typeof ($scope.Invoice.bominvoiceitems) == 'undefined')
                $scope.Invoice.bominvoiceitems = [];
            $scope.ItemDetails.buyingqty = qty;
            $scope.fixTax();
            $scope.ItemDetails.buyingamount = ($scope.GetNumber($scope.ItemDetails.basebuyingprice) * $scope.GetNumber($scope.ItemDetails.buyingqty)).round(2);

            for (var i = 0; i < pieces; i++) {
                var newitem = JSON.parse(JSON.stringify($scope.ItemDetails));
                newitem.sno = newitem.sno + (i / 10);
                $scope.Invoice.bominvoiceitems.push(newitem);
            }
            $scope.CalculateTotal();
        };

        $scope.RowValuechanged = function (item) {

            if (item.purchasediscount.toString().indexOf('%') > 0) {
                item.purchasediscount = item.purchasediscount.toString().replace('%', '');
                item.purchasediscount = ($scope.GetNumber(item.buyingprice) * $scope.GetNumber(item.purchasediscount) / 100).round(2);
            }
            item.buyingprice = item.basebuyingprice - item.purchasediscount
            var taxitem = $scope.purchaseTaxActual.find(function (pitem) {
                return (pitem.id == item.taxid);
            });
            if (taxitem || taxitem != null) {
                taxitem.taxsplit.findTax = $scope.findTax;
                var taxconfig = taxitem.taxsplit.findTax(item.buyingprice);
                if (taxconfig.cgst == 0) {
                    taxconfig = { 'cgst': 0, 'sgst': 0 };
                }
                item.purchasetaxpercentage = taxconfig.cgst + taxconfig.sgst;
            }

            item.amount = $scope.GetNumber(item.basebuyingprice) * $scope.GetNumber(item.buyingqty);
            item.purchasetotaldiscount = ($scope.GetNumber(item.basebuyingprice) * $scope.GetNumber(item.buyingqty) * item.purchasediscountpercentage / 100).round(2);
            item.amount -= item.purchasetotaldiscount;
            item.purchasetax = (item.amount * item.purchasetaxpercentage / 100).round(2);
            item.purchasediscount = (item.purchasetotaldiscount / $scope.GetNumber(item.buyingqty)).round(2);
            $scope.CalculateTotal();
        }

        $scope.AddItemInternal = function (computetotal) {
            $scope.ItemDetails.sno = parseFloat($scope.ItemDetails.sno);
            $scope.ItemDetails.amount = $scope.GetNumber($scope.ItemDetails.basebuyingprice) * $scope.GetNumber($scope.ItemDetails.buyingqty);
            $scope.ItemDetails.purchasetotaldiscount = ($scope.GetNumber($scope.ItemDetails.basebuyingprice) * $scope.GetNumber($scope.ItemDetails.buyingqty) * $scope.ItemDetails.purchasediscountpercentage / 100).round(2);
            $scope.ItemDetails.amount -= $scope.ItemDetails.purchasetotaldiscount;
            $scope.ItemDetails.purchasediscount = ($scope.ItemDetails.purchasetotaldiscount / $scope.GetNumber($scope.ItemDetails.buyingqty)).round(2);

            var prditem = null;

            if (!$scope.mfg_singlepo) {
                prditem = $scope.BOMList.find(function (item) {
                    return (item.id == $scope.ItemDetails.bomid);
                });
            }
            else {
                prditem = $scope.BOMAllList.find(function (item) {
                    return (item.id == $scope.ItemDetails.bomid);
                });
            }

            if (prditem || prditem != null) {
                $scope.ItemDetails.taxid = prditem.taxid;
            }

            var taxitem = $scope.purchaseTaxActual.find(function (pitem) {
                return (pitem.id == $scope.ItemDetails.taxid);
            });
            if (taxitem || taxitem != null) {
                taxitem.taxsplit.findTax = $scope.findTax;
                var taxconfig = taxitem.taxsplit.findTax($scope.ItemDetails.buyingprice);
                if (taxconfig.cgst == 0) {
                    taxconfig = { 'cgst': 0, 'sgst': 0 };
                }
                $scope.ItemDetails.purchasetaxpercentage = taxconfig.cgst + taxconfig.sgst;
            }


            $scope.ItemDetails.purchasetax = ($scope.ItemDetails.amount * $scope.ItemDetails.purchasetaxpercentage / 100).round(2);


            if (isNaN($scope.ItemDetails.sno)) {
                if ($scope.Invoice.bominvoiceitems.length == 0)
                    $scope.ItemDetails.sno = 1;
                else
                    $scope.ItemDetails.sno = $scope.Invoice.bominvoiceitems[0].sno + 1;
            }

            if (typeof ($scope.Invoice.bominvoiceitems) == 'undefined')
                $scope.Invoice.bominvoiceitems = [];

            var currentdate = new Date();
            $scope.ItemDetails.uniqueid = currentdate.getTime();
            if ($scope.Invoice.bominvoiceitems.indexOf($scope.ItemDetails) < 0) {
                if ($scope.Invoice.bominvoiceitems.length > 0) {
                    var index = 0;
                    $.each($scope.Invoice.bominvoiceitems, function (k, v) {
                        if (v.sno < $scope.ItemDetails.sno) {
                            index = k;
                        }
                    });
                    if (index + 1 < $scope.Invoice.bominvoiceitems.length) {
                        if ($scope.Invoice.bominvoiceitems[index + 1].sno == $scope.ItemDetails.sno) {
                            index += 1;
                            while (index < $scope.Invoice.bominvoiceitems.length &&
                                $scope.Invoice.bominvoiceitems[index].sno == $scope.ItemDetails.sno) {
                                index += 1;
                            }
                        }
                        if (index < $scope.Invoice.bominvoiceitems.length)
                            $scope.Invoice.bominvoiceitems.splice(index + 1, 0, $scope.SetRowStyle($scope.ItemDetails));
                        else
                            $scope.Invoice.bominvoiceitems.push($scope.SetRowStyle($scope.ItemDetails));
                    }
                    else {
                        $scope.Invoice.bominvoiceitems.push($scope.SetRowStyle($scope.ItemDetails));
                    }
                }
                else {
                    $scope.Invoice.bominvoiceitems.push($scope.ItemDetails);
                }
            }
            if (typeof (computetotal) == 'undefined' || computetotal)
                $scope.CalculateTotal();
        };

        $scope.SetRowStyle = function (item) {
            if (typeof (item.poinfo) != 'undefined' &&
                item.poinfo != null && item.poinfo.length > 0)
                item.style = { "background-color": "lightgrey" };
            else
                item.style = {};
            return item;

        }

        $scope.RoundPrice = function (amount) {
            return amount.round(0);

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

        $scope.discountChanged = function () {
            var cost = parseFloat($scope.ItemDetails.basebuyingprice);
            if (isNaN(cost)) cost = 0;
            if ($scope.ItemDetails.purchasediscount &&
                $scope.ItemDetails.purchasediscount != null &&
                $scope.ItemDetails.purchasediscount != '') {
                if ($scope.ItemDetails.purchasediscount.toString().endsWith('%')) {
                    $scope.ItemDetails.purchasediscountpercentage = parseFloat($scope.ItemDetails.purchasediscount.toString().replace('%', ''));
                    if (isNaN($scope.ItemDetails.purchasediscountpercentage)) $scope.ItemDetails.purchasediscountpercentage = 0;
                    $scope.ItemDetails.purchasediscount = (cost * $scope.ItemDetails.purchasediscountpercentage / 100).round(2);
                }
                else {
                    $scope.ItemDetails.purchasediscount = parseFloat($scope.ItemDetails.purchasediscount.toString());
                    if (isNaN($scope.ItemDetails.purchasediscount)) $scope.ItemDetails.purchasediscount = 0;
                    $scope.ItemDetails.purchasediscountpercentage = ($scope.ItemDetails.purchasediscount / cost*100).round(2);
                }
            }
            else {
                $scope.ItemDetails.purchasediscount = parseFloat($scope.ItemDetails.purchasediscount.toString());
                if (isNaN($scope.ItemDetails.purchasediscount)) $scope.ItemDetails.purchasediscount = 0;
                $scope.ItemDetails.purchasediscountpercentage = ($scope.ItemDetails.purchasediscount / cost * 100).round(2);
            }
            $scope.marginChanged();
        }

        $scope.marginChanged = function () {
            var basecost = $scope.GetNumber($scope.ItemDetails.basebuyingprice) - $scope.GetNumber($scope.ItemDetails.purchasediscount); 
            $scope.ItemDetails.sellingprice = basecost + (basecost * $scope.GetNumber($scope.ItemDetails.marginpercentage) / 100).round(2);
        };

        $scope.calculatesalestax = function () {
            var taxitem = $scope.CurrentTaxConfig;
            if (taxitem == null) {
                $scope.ItemDetails.salestaxpercentage = 0;
            }
            else {
                var taxconfig = taxitem.taxsplit.findTax($scope.GetNumber($scope.ItemDetails.sellingprice) - $scope.ItemDetails.salediscount);
                $scope.ItemDetails.salestaxpercentage = taxconfig.cgst + taxconfig.sgst;
            }

            $scope.ItemDetails.salestax = ($scope.GetNumber($scope.ItemDetails.buyingqty) * ($scope.GetNumber($scope.ItemDetails.sellingprice) - $scope.ItemDetails.salediscount) / ($scope.ItemDetails.salestaxpercentage + 100) * $scope.ItemDetails.salestaxpercentage).round(2);
        };



        $scope.CalculateTotal = function () {
            $scope.Total = {
                "buyingqty": 0,
                "buyingpiece": 0,
                "mrp": 0,
                "purchasediscount": 0,
                "sellingprice": 0,
                "buyingprice": 0,
                "basebuyingprice": 0,
                "tax": 0,
                "amount": 0
            };

            $.each($scope.Invoice.bominvoiceitems, function (k, v) {



                v.buyingqty = parseFloat(v.buyingqty);
                v.buyingprice = parseFloat(v.buyingprice);
                v.sellingprice = parseFloat(v.sellingprice);

                if (isNaN(v.buyingprice)) v.buyingprice = 0.0;
                if (isNaN(v.buyingqty)) v.buyingqty = 0.0;
                if (isNaN(v.sellingprice)) v.sellingprice = 0.0;

                $scope.Total.mrp += (v.mrp * v.buyingqty);
                $scope.Total.buyingprice += (v.buyingprice * v.buyingqty);
                $scope.Total.basebuyingprice += (v.basebuyingprice * v.buyingqty);
                $scope.Total.purchasediscount += v.purchasetotaldiscount;
                $scope.Total.sellingprice += (v.sellingprice * v.buyingqty);
                $scope.Total.buyingqty += v.buyingqty;
                $scope.Total.tax += v.purchasetax;
                $scope.Total.amount += v.amount;

            });

            $scope.Total.tax = $scope.Total.tax.round(2);
            $scope.Total.amount = $scope.Total.amount.round(2);
            $scope.Total.mrp = $scope.Total.mrp.round(2);
            $scope.Total.buyingprice = $scope.Total.buyingprice.round(2);
            $scope.Total.sellingprice = $scope.Total.sellingprice.round(2);
            $scope.Total.buyingqty = $scope.Total.buyingqty.round(2);

            $scope.Invoice.baseamount = $scope.Total.buyingprice.round(2);
            $scope.Invoice.grossamount = $scope.Total.basebuyingprice.round(2);
            $scope.Invoice.discount = $scope.Total.purchasediscount.round(2);
        };

        $scope.InvoiceList = [];
        $scope.ShowInvoiceList = false;

        $scope.InitializeInvoiceList = function (loaddirect) {

            request = {
                "Item": { id: $scope.InvoiceInfo.invoiceid },
                "SecurityToken": "String content"
            }

            if (!loaddirect && typeof ($scope.InvoiceInfo.invoiceid) != 'undefined' && $scope.InvoiceInfo.invoiceid.indexOf(',') > 0) {
                request.Item.ids = $scope.InvoiceInfo.invoiceid;
                request.Item.id = 0;
            }

            $http({
                method: 'POST',
                url: "BOMinvoiceService/Search",
                data: request
            }).success(function (result) {

                if (typeof (request.Item.ids) != 'undefined') {
                    $scope.InvoiceList = result;
                    $scope.ShowInvoiceList = true;
                }
                else {
                    $scope.DisplayInvoice(result.Items[0]);
                    $scope.Invoice.lrid = result.lrid;
                    $scope.Invoice.lrno = result.lrno;
                    $scope.Invoice.lrdate = result.lrdate;
                    $scope.Invoice.lrentryno = result.BOMLrentryno;
                    $scope.Invoice.invoiceno = result.Items[0].invoiceno;
                    $scope.Invoice.status = result.status;
                    $scope.Invoice.supplierid = result.supplierid;
                    $scope.Invoice.suppliercompanyname = result.suppliercompanyname;
                    $scope.Invoice.companyname = result.companyname;
                    $scope.Invoice.transportid = result.transportid;
                    $scope.Invoice.retaillocationid = result.retaillocationid;
                    $scope.Invoice.bundles = result.noofbundels;
                    $scope.Invoice.lrnoInfo = result.lrentryno + " / " + result.lrno;
                    $scope.ItemDetails = (JSON.parse(JSON.stringify($scope.ItemTemplate)));
                    $scope.CalculateTotal();
                    if ($scope.Invoice.bominvoiceitems.length > 0) {
                        $scope.ItemDetails.sno = $scope.Invoice.bominvoiceitems[$scope.Invoice.bominvoiceitems.length - 1].sno + 1;
                    }
                    $timeout(function () {
                        $scope.ConfigureEditGrid();
                        $scope.egrid.UpdateData($scope.Invoice.bominvoiceitems);
                    }, 10);
                }
            }).error(function (result) {
            });
        };

        if (typeof ($scope.InvoiceInfo.invoiceid) == 'undefined') {
            $scope.SwitchPanel();
        }
        else if ($scope.InvoiceInfo.invoiceid > 0 || $scope.InvoiceInfo.invoiceid.indexOf(',') > 0) {
            $scope.InitializeInvoiceList(false);
        }

        $scope.SupplierChanged = function () {
            if (typeof ($scope.Invoice.supplierid) != 'undefined' ||
                $scope.Invoice.supplierid != null) {
                $scope.Invoice.suppliercompanyid = $scope.SupplierCompany.find(function (item) {
                    return item.supplierid == $scope.Invoice.supplierid;
                }).id;
            }
            $scope.SearchPO();
        }

        $scope.focusconditiononenter = function (issuccess, sfocus, ffocus) {
            $scope[issuccess ? 'setFocus' + sfocus : 'setFocus' + ffocus]();
        }

        $scope.currentcomponentorderitemchanged = function () {
            var orderitem = $scope.SinglePOListItems.find(function (item) { return item.componentpoitemsid == $scope.componentpoitemsid; });
            if (orderitem == undefined || orderitem == null) return;

            if ($scope.ItemDetails == undefined || $scope.ItemDetails == null)
                $scope.ItemDetails = (JSON.parse(JSON.stringify($scope.ItemTemplate)));

            $scope.ItemDetails.bomcategoryid = orderitem.bomcategoryid;
            $scope.ItemDetails.bomid = orderitem.bomid;
            $scope.ItemDetails.colourid = orderitem.colourid;
            $scope.ItemDetails.width = orderitem.width;
            $scope.ItemDetails.basebuyingprice = orderitem.rate;
            $scope.ItemDetails.buyingqty = orderitem.qty - (orderitem.cqty + orderitem.rqty);
            $scope.ItemDetails.purchasediscount = 0;
            $scope.ItemDetails.marginpercentage = 0;
            $scope.marginChanged();

            $scope.ItemDetails.poinfo = [];
            $scope.ItemDetails.poinfo.push(
                {
                    'componentpoitemsid': orderitem.componentpoitemsid,
                    'pono': orderitem.pono,
                    'aqty': 0,
                    'rate': orderitem.rate,
                    'oqty': orderitem.qty
                });
        }

        $scope.POChanged = function () {
            $scope.SelectedPO = $scope.SinglePOList.find(function (item) { return item.componentorderid == $scope.currentcomponentorderid });
            if ($scope.SelectedPO && $scope.SelectedPO != null) {
                $scope.SinglePOListItems = $scope.SelectedPO.items;
            }
            else {
                $scope.SinglePOListItems = [];
            }
        }

        $scope.SearchPO = function (callback) {

            var request = {
                "Item": { 'supplierid': $scope.Invoice.supplierid, 'invoiceid': $scope.Invoice.id },
                "SecurityToken": "String content"
            }

            $http({
                method: 'POST',
                url: "ComponentPOService/SearchPO",
                data: request
            }).success(function (result) {
                if (typeof (callback) == 'undefined') {
                    $scope.CurrentPOList = result;

                    if ($scope.mfg_singlepo) {
                        var polist = [];
                        $.each($scope.CurrentPOList, function (k, v) {
                            var exist = polist.find(function (item) { return item.componentorderid == v.componentorderid; });
                            if (exist == undefined || exist == null) {
                                exist = { 'componentorderid': v.componentorderid, 'pono': v.pono, 'items': [] };
                                polist.push(exist);
                            }

                            var display = [];
                            if (v.bomcategoryid > 0) {
                                var bomcat = $scope.BOMCategories.find(function (item) {
                                    return item.id == v.bomcategoryid;
                                });
                                if (bomcat && bomcat != null) {
                                    display.push(bomcat.name);
                                }
                            }

                            if (v.bomid > 0) {
                                var bomcat = $scope.BOMAllList.find(function (item) {
                                    return item.id == v.bomid;
                                });
                                if (bomcat && bomcat != null) {
                                    display.push(bomcat.name);
                                }
                            }

                            if (v.colourid > 0) {
                                var bomcat = $scope.AllColorList.find(function (item) {
                                    return item.id == v.colourid;
                                });
                                if (bomcat && bomcat != null) {
                                    display.push(bomcat.name);
                                }
                            }
                            v.display = display.join(' / ') + '(' + v.componentpoitemsid + ')';
                            exist.items.push(v);
                        })
                        $scope.SinglePOList = polist;
                        $scope.SinglePOListItems = [];
                    }else {
                        $scope.UpdatePOList();
                        if ($scope.Invoice &&
                            $scope.Invoice.bominvoiceitems &&
                            $scope.Invoice.bominvoiceitems != null) {
                            $.each($scope.Invoice.bominvoiceitems, function (k, v) {
                                if (v.poinfo &&
                                    v.poinfo != null &
                                    v.poinfo.length > 0) {
                                    $.each(v.poinfo, function (kk, vv) {
                                        var order = result.find(function (item) {
                                            return item.componentpoitemsid == vv.componentpoitemsid;
                                        });
                                        if (order && order != null) {
                                            vv.rate = order.rate;
                                        }
                                    });
                                }
                            });
                            if ($scope.egrid && $scope.egrid != null)
                                $scope.egrid.UpdateData($scope.Invoice.bominvoiceitems);
                        }
                    }
                }
                else {
                    callback(result);
                }
            }).error(function (result) {

            });
        };

        $scope.Save = function (movenext) {
            if ($scope.Invoice.totalamount != ($scope.Total.amount + $scope.Total.tax + $scope.GetNumber($scope.Invoice.otheramount))) {
                var diffvalue = Math.abs(Math.abs($scope.Invoice.totalamount) - Math.abs($scope.Total.amount + $scope.Total.tax + $scope.GetNumber($scope.Invoice.otheramount)));
                if (diffvalue > 1) {
                    $rootScope.Confirm('Invoice Value Mismatch', diffvalue + ' amount different in invoice amount do you want to continue', function () {
                        $scope.ActualSave(movenext);
                    });
                }
                else {
                    $scope.ActualSave(movenext);
                }
            }
            else {
                $scope.ActualSave(movenext);
            }
        }

        $scope.ActualSave = function (movenext) {


            if (typeof ($scope.Invoice.companyid) == 'undefined' || $scope.Invoice.companyid == null || $scope.Invoice.companyid == 0) {
                $scope['setFocuscompanyid']();
                $rootScope.Toaster.Info("Please select buying company name");
                return;
            }

            if ($scope.Invoice.invoiceno == null || $scope.Invoice.invoiceno == 0) {
                $scope['setFocusinvoiceno']();
                $rootScope.Toaster.Info("Please select Invoice No");
                return;
            }
            if ($scope.Invoice.invoicedate == null || $scope.Invoice.invoicedate == "") {
                $scope['setFocusinvoicedate']();
                $rootScope.Toaster.Info("Please select Invoice Date");
                return;
            }
            if ($scope.Invoice.locationid == 'undefined' || $scope.Invoice.locationid == null || $scope.Invoice.locationid == 0) {
                $scope['setFocuslocationid']();
                $rootScope.Toaster.Error("Please select Receiving Location");
                return;
            }

            if ($scope.Invoice.totalamount == null || $scope.Invoice.totalamount == 0) {
                $scope['setFocusbillvalue']();
                $rootScope.Toaster.Info("Please enter Bill value");
                return;
            }

            if ($scope.SaveRequest) return;
            $scope.SaveRequest = true;
            $scope.Invoice.isdirectpurchase = true;

            var request = {
                "Item": $scope.Invoice,
                "securitytoken": "String content"
            }
            var srequest = JSON.stringify(request);
            $http({
                method: 'POST',
                url: "BOMinvoiceService/SaveInvoiceItems",
                data: srequest
            }).success(function (result) {
                $scope.SaveRequest = false;
                if (result != null && result.id > 0) {
                    //var invoiceids = result.id;


                    $rootScope.Toaster.Info("InvoiceItems Saved Successfully.!");
                    $scope.DisplayInvoice(result);
                    $scope.InvoiceItem = (JSON.parse(JSON.stringify($scope.ItemTemplate)));

                }
                else {
                    $rootScope.Toaster.Error(result.ErrorMessage);
                    $scope.ErrorStyle = "danger";
                }

            }).error(function (result) {
                $scope.SaveRequest = false;
            });
        }

        $scope.DisplayInvoice = function (invoice) {
            $scope.Navigation.SetMode('EDIT');
            $scope.Invoice = invoice;
            if ($scope.Invoice.bominvoiceitems.length > 0) {
                $scope.ItemDetails.sno = $scope.Invoice.bominvoiceitems[$scope.Invoice.bominvoiceitems.length - 1].sno + 1;
                $.each($scope.Invoice.bominvoiceitems, function (k, v) {
                    if (typeof (v.buyingamount) == 'undefined') {
                        v.buyingamount = (v.basebuyingprice * v.buyingqty).round(2);
                    }
                });
            }

            $scope.CalculateTotal();
            $scope.ComputeTaxTotal();
            $scope.additionaldiscountpercentage = 0;
            $.each($scope.Invoice.bominvoicetaxitems, function (k, v) {
                if (v.type > 0) {
                    $scope.additionaldiscountpercentage += v.discountpercentage;
                    //v.percentage = 'Rounding';
                }
            });
            $scope.SearchPO();
            $timeout(function () {
                $scope.ShowSearch = false;
                $scope.ConfigureEditGrid();
                $scope.egrid.UpdateData($scope.Invoice.bominvoiceitems);
            }, 10);
        }

        $scope.ManageSearchItem = function (action, item) {
            if (action == "Delete") {


                $scope.Total.mrp -= (item.mrp * item.buyingqty);
                $scope.Total.buyingprice -= (item.buyingprice * item.buyingqty);
                $scope.Total.sellingprice -= (item.sellingprice * item.buyingqty);
                $scope.Total.buyingqty -= item.buyingqty;
                $scope.Total.tax -= item.purchasetax;
                $scope.Total.purchasediscount -= item.purchasetotaldiscount;
                $scope.Total.amount -= item.amount;


                $scope.Invoice.bominvoiceitems.splice($scope.Invoice.bominvoiceitems.indexOf(item), 1);

                if ($scope.Invoice.bominvoiceitems.length == 0) {
                    $scope.ItemDetails.sno = 0;
                    $scope.Total = {
                        "buyingqty": 0,
                        "mrp": 0,
                        "sellingprice": 0,
                        "buyingprice": 0,
                        "tax": 0,
                        "amount": 0
                    };
                }
                else {
                    $scope.ItemDetails.sno = $scope.Invoice.bominvoiceitems[$scope.Invoice.bominvoiceitems.length - 1].sno;
                    $scope.Total.tax = $scope.Total.tax.round(2);
                    $scope.Total.amount = $scope.Total.amount.round(2);
                    if ($scope.Total.discount && $scope.Total.discount!=null)
                    $scope.Total.discount = $scope.Total.discount.round(2);
                    $scope.Total.mrp = $scope.Total.mrp.round(2);
                    $scope.Total.buyingprice = $scope.Total.buyingprice.round(2);
                    $scope.Total.sellingprice = $scope.Total.sellingprice.round(2);
                    $scope.Total.buyingqty = $scope.Total.buyingqty.round(2);
                }
                $scope.egrid.UpdateData($scope.Invoice.bominvoiceitems);
            }
            else if (action == "Edit") {
                if (typeof ($scope.ItemDetails.IsEditItem) != 'undefined' &&
                    $scope.ItemDetails.IsEditItem) {
                    $rootScope.Confirm('Editing', 'you will loose the current editing item', function () {
                        $scope.EditItem(item);
                        $scope.ItemDetails.IsEditItem = true;
                    });
                }
                else {
                    $scope.EditItem(item);
                    $scope.ItemDetails.IsEditItem = true;
                }
                $scope.egrid.UpdateData($scope.Invoice.bominvoiceitems);
            }
            else if (action == "EditInvoice") {
                $scope.InvoiceInfo.invoiceid = item.id;
                $scope.InitializeInvoiceList(true);
            }
            else if (action == "DeleteInvoice") {
                $.confirm({
                    title: 'Delete',
                    content: 'Do you want to delete the selected purchase entry?',
                    confirmButton: 'Yes',
                    confirmButtonClass: 'btn-info',
                    icon: 'fa fa-question-circle',
                    animation: 'scale',
                    animationClose: 'top',
                    opacity: 0.5,
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
                                    url: "BOMinvoiceService/Delete",
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
                        this.$content.find('form').on('submit', function (e) {
                            // if the user submits the form by pressing enter in the field.
                            e.preventDefault();
                            jc.$$formSubmit.trigger('click'); // reference the button and click it
                        });
                    }
                });
            }
        }

        $scope.PrintInvoice = function (id) {
            $timeout(function () {
                var link = document.createElement('a');
                link.download = "po.pdf";
                link.href = 'BOMinvoiceService/PrintPDF/' + $scope.UserInfo.OrganizationID + '/' + id;
                document.body.appendChild(link);
                link.click();
                $timeout(function () {
                    link.remove();
                }, 100);
            }, 10);
        }

        $scope.EditItem = function (item) {

            $scope.Total.mrp -= (item.mrp * item.buyingqty);
            $scope.Total.buyingprice -= (item.buyingprice * item.buyingqty);
            $scope.Total.sellingprice -= (item.sellingprice * item.buyingqty);
            $scope.Total.buyingqty -= item.buyingqty;
            $scope.Total.tax -= item.purchasetax;
            $scope.Total.purchasediscount -= item.purchasetotaldiscount;
            $scope.Total.amount -= item.amount;


            $scope.Invoice.bominvoiceitems.splice($scope.Invoice.bominvoiceitems.indexOf(item), 1);


            $scope.Total.purchasediscount = $scope.Total.purchasediscount.round(2);
            $scope.Total.tax = $scope.Total.tax.round(2);
            $scope.Total.amount = $scope.Total.amount.round(2);
            $scope.Total.mrp = $scope.Total.mrp.round(2);
            $scope.Total.buyingprice = $scope.Total.buyingprice.round(2);
            $scope.Total.sellingprice = $scope.Total.sellingprice.round(2);
            $scope.Total.buyingqty = $scope.Total.buyingqty.round(2);
            $scope.ItemDetails = item;
            var bomid = item.bomid;
            var colourid = item.colourid;
            var bomcategoryid = item.bomcategoryid;
            $scope.categoryChange();
            $scope.ItemDetails.bomid = bomid;
            $scope.productChange();
            $scope.ItemDetails.bomcategoryid = null;
            $scope.ItemDetails.bomid = null;
            $scope.ItemDetails.colourid = null;
            $timeout(function () {
                $scope.ItemDetails.bomcategoryid = bomcategoryid;
                $scope.ItemDetails.bomid = bomid;
                $scope.ItemDetails.colourid = colourid;
            }, 10);
            $scope.taxChange();
            $scope.egrid.UpdateData($scope.Invoice.bominvoiceitems);
        }

        $scope.DeleteArrayItem = function (source, list) {
            list.splice(list.indexOf(source), 1);
            $scope.ComputeTaxTotal();
        }

        $scope.mrpChange = function () {
            var value = $scope.ItemDetails.mrp;
            $scope.ItemDetails.sellingprice = value;
            if ($scope.ItemDetails.buyingprice == null || isNaN($scope.ItemDetails.buyingprice))
                $scope.ItemDetails.buyingprice = $scope.GetNumber($scope.ItemDetails.basebuyingprice);

            $scope.ItemDetails.marginpercentage = (($scope.GetNumber($scope.ItemDetails.sellingprice) / $scope.GetNumber($scope.ItemDetails.basebuyingprice) * 100) - 100).round(2);

            var taxitem = $scope.CurrentTaxConfig;

            if (taxitem == null) {
                $scope.ItemDetails.salestaxpercentage = 0;
            }
            else {
                var taxconfig = taxitem.taxsplit.findTax($scope.GetNumber($scope.ItemDetails.sellingprice) - $scope.ItemDetails.salediscount);
                $scope.ItemDetails.salestaxpercentage = taxconfig.cgst + taxconfig.sgst;
            }
        }

        $scope.UpdatePOList = function (fromqty) {
            if (typeof ($scope.ItemDetails.bomcategoryid) == 'undefined' ||
                typeof ($scope.ItemDetails.bomid) == 'undefined') return;

            if ($scope.mfg_singlepo) {
                if (fromqty) {
                    if ($scope.ItemDetails.poinfo == undefined ||
                        $scope.ItemDetails.poinfo == null ||
                        $scope.ItemDetails.poinfo.length == 0)
                        return;
                    $scope.ItemDetails.poinfo[0].aqty = $scope.GetNumber($scope.ItemDetails.buyingqty);
                }
                return;
            }

            var orderitems = [];
            $.each($scope.CurrentPOList, function (k, v) {
                if (v.bomid == $scope.ItemDetails.bomid) {
                    if (v.colourid <= 0) {
                        orderitems.push(v);
                    }
                    else if ($scope.ItemDetails.colourid && $scope.ItemDetails.colourid > 0) {
                        if ($scope.ItemDetails.colourid == v.colourid)
                            orderitems.push(v);
                    }
                }
            });

            if ($scope.GetNumber($scope.ItemDetails.buyingqty) > 0) {
                $scope.ItemDetails.poinfo = [];
                var result = [];
                var accepted = 0;
                var availble = $scope.GetNumber($scope.ItemDetails.buyingqty);
                $.each(orderitems, function (k, v) {
                    if (availble > 0) {
                        if (availble > v.balance) {
                            result.push(v.pono + ':' + v.balance);
                            availble -= v.balance;
                            $scope.ItemDetails.poinfo.push(
                                {
                                    'componentpoitemsid': v.componentpoitemsid,
                                    'pono': v.pono,
                                    'aqty': v.balance,
                                    'oqty': v.qty,
                                    'rate':v.rate,
                                });
                            accepted += v.balance;
                        }
                        else {
                            result.push(v.pono + ':' + availble + '(' + v.qty + ')');
                            $scope.ItemDetails.poinfo.push(
                                {
                                    'componentpoitemsid': v.componentpoitemsid,
                                    'pono': v.pono,
                                    'aqty': availble,
                                    'rate': v.rate,
                                    'oqty': v.qty
                                });
                            accepted += availble;
                            availble = 0;
                        }
                    }
                });
                $scope.ItemDetails.polist = result.join('/');
            }
            else {
                $scope.ItemDetails.polist = 'Select BOM to link';
            }
        }


        $scope.ShowPOItems = function (ItemDetails, edit, callback) {

            var orderitems = [];
            $.each($scope.CurrentPOList, function (k, v) {
                var exist = orderitems.find(function (item) {
                    return item.componentorderid == v.componentorderid;
                })
                if (exist == undefined || exist == null) {
                    exist = {
                        'componentorderid': v.componentorderid,
                        'podate': v.podate,
                        'pono': v.componentorderid,
                        'oqty': 0,
                        'pqty': 0,
                        'items':[]
                    };
                    orderitems.push(exist)
                }
                exist.oqty += v.qty;
                exist.pqty += (v.qty-v.rqty-v.cqty);
                exist.items.push(v);
            });

            if (orderitems.length == 0) {
                $scope.Toaster.Error("There is no Purchase order matching for the Invoice Item.");
                if (typeof (callback) != 'undefined')
                    callback(false);
                return;
            }


            var dialog = ngDialog.open({
                template: 'POPending',
                data: [$scope, orderitems],
                width: '850px',
                controller: ['$scope', '$timeout', function ($scope, $timeout) {
                    $scope.Parent = $scope.ngDialogData[0];
                    $scope.PopupGroup = $scope.ngDialogData[1];;
                    $scope.PopupList = [];
                    $scope.ngDialogData[1];
                    $scope.FillQty = function () {
                        $.each($scope.PopupList, function (k, v) {
                            v.aqty = v.qty - v.rqty - v.cqty;
                        });
                    }
                    $scope.ShowDetail = function (item) {
                        $scope.PopupList = item.items;
                    }
                    $scope.Selected = function (item) {
                        $scope.closeThisDialog($scope.PopupList);
                    };
                }],
                className: 'ngdialog-theme-plain'
            });
            dialog.closePromise.then(function (data) {
                {
                    if (typeof (data.value) != 'undefined' && data.value != null && data.value != '$escape'
                        && data.value != '$document') {
                        $.each(data.value, function (k, v) {
                            if ($scope.GetNumber(v.aqty) > 0) {
                                var alist = [];
                                var oqty = 0;
                                var eMsg = [];
                                $scope.CreateNew();
                                $scope.ItemDetails.poinfo = [];
                                $scope.ItemDetails.poinfo.push({
                                    'componentpoitemsid': v.componentpoitemsid,
                                    'pono': v.pono,
                                    'aqty': $scope.GetNumber(v.aqty),
                                    'rate': v.rate,
                                    'oqty': v.qty
                                });
                                $scope.ItemDetails.bomcategoryid = v.bomcategoryid;
                                $scope.ItemDetails.bomid = v.bomid;
                                $scope.ItemDetails.colourid = v.colourid;
                                $scope.ItemDetails.width = v.width;
                                $scope.ItemDetails.buyingqty = v.aqty;
                                $scope.ItemDetails.basebuyingprice = v.rate;
                                alist.push(v.pono + ':' + v.aqty);
                                $scope.ItemDetails.polist = alist.join('/');
                                $scope.marginChanged();
                                $scope.BOMList = $scope.BOMAllList;
                                $scope.AddItem();
                            }
                        });
                    }

                    if (typeof (callback) != 'undefined') {
                        callback(false);
                    }
                }
            });
        }


        $scope.AllocatePOItems = function (ItemDetails, edit, callback) {

            var orderitems = [];
            $.each($scope.CurrentPOList, function (k, v) {
                if (v.bomid == ItemDetails.bomid) {
                    if (v.colourid <= 0) {
                        orderitems.push(v);
                    }
                    else if (ItemDetails.colourid && ItemDetails.colourid > 0) {
                        if (ItemDetails.colourid == v.colourid)
                            orderitems.push(v);
                    }
                }
            });

            if (orderitems.length == 0) {
                $scope.Toaster.Error("There is no Purchase order matching for the Invoice Item.");
                if (typeof (callback) != 'undefined')
                    callback(false);
                return;
            }

            if (ItemDetails.poinfo != null && ItemDetails.poinfo.length > 0) {
                $.each(ItemDetails.poinfo, function (k, v) {
                    var existingitem = orderitems.find(function (item) {
                        return item.componentpoitemsid == v.componentpoitemsid;
                    });
                    if (typeof (existingitem) != 'undefined' && existingitem != null) {
                        existingitem.aqty = v.aqty;
                    }
                });
            }

            var title = $scope.SearchRef(ItemDetails.bomid, $scope.BOMAllList);
            var dialog = ngDialog.open({
                template: 'POAllocation',
                data: [$scope, orderitems, title, ItemDetails.buyingqty],
                width: '750px',
                controller: ['$scope', '$timeout', function ($scope, $timeout) {
                    $scope.Title = $scope.ngDialogData[2];
                    $scope.PopupList = $scope.ngDialogData[1];
                    $scope.InvoiceQty = $scope.ngDialogData[3];
                    $scope.Allocate = function () {
                        var aqty = parseFloat($scope.InvoiceQty);
                        if (isNaN(aqty)) aqty = 0;
                        $.each($scope.PopupList, function (k, v) {
                            v.aqty = 0;
                        });
                        for (var i = 0; i < $scope.PopupList.length && aqty > 0; i++) {
                            if ($scope.PopupList[i].qty - $scope.PopupList[i].rqty > 0) {
                                var remqty = $scope.PopupList[i].qty - $scope.PopupList[i].rqty;
                                if (remqty > aqty) {
                                    $scope.PopupList[i].aqty = aqty;
                                } else {
                                    $scope.PopupList[i].aqty = remqty;
                                }
                                aqty -= $scope.PopupList[i].aqty;
                            }
                        }
                    }
                    $scope.Selected = function (item) {
                        $scope.closeThisDialog($scope.PopupList);
                    };
                }],
                className: 'ngdialog-theme-plain'
            });
            dialog.closePromise.then(function (data) {
                {
                    if (typeof (data.value) != 'undefined' && data.value != null && data.value != '$escape'
                        && data.value != '$document') {
                        ItemDetails.poinfo = [];
                        var alist = [];
                        var oqty = 0;
                        var eMsg = [];
                        $.each(data.value, function (k, v) {
                            if ($scope.GetNumber(v.aqty) > 0) {
                                ItemDetails.poinfo.push({
                                    'componentpoitemsid': v.componentpoitemsid,
                                    'pono': v.pono,
                                    'aqty': $scope.GetNumber(v.aqty),
                                    'rate': v.rate,
                                    'oqty': v.qty
                                });

                                alist.push(v.pono + ':' + v.aqty);
                                oqty += $scope.GetNumber(v.aqty);
                            }
                        });

                        if (!edit && (typeof (ItemDetails.buyingqty) == 'undefined' ||
                            $scope.GetNumber(ItemDetails.buyingqty) != oqty)) {
                            ItemDetails.buyingqty = oqty;
                        }
                        if (alist.length > 0) {
                            ItemDetails.polist = alist.join('/');
                        }
                    }

                    if (typeof (callback) != 'undefined') {
                        callback(false);
                    }
                }
            });
        }

        $scope.Search = function () {

            request = {
                "Item": $scope.SearchCondition,
                "securitytoken": "String content"
            }

            var srequest = JSON.stringify(request);

            $http({
                method: 'POST',
                url: "BOMinvoiceService/Search",
                data: srequest
            }).success(function (result) {
                if (typeof ($scope.SearchCondition.lrid) == 'undefined' || $scope.SearchCondition.lrid == 0) {
                    $scope.SearchResult = result;
                }
                else {
                    $scope.SearchResult = result.Items;
                    $scope.SearchCondition.lrid = 0;
                }
                $scope.LoadGrid();
            }).error(function (result) {
            });

        }
        $scope.LoadGrid = function () {
            if ($scope.igrid != null)
                $scope.igrid.destroy();

            var igrid = new InfyGrid('gridContainer', '450px', '100%', true, true, true, 0);
            igrid.options.showHeaderRow = true;
            igrid.EnableSerial($scope.SearchResult);

            igrid.AddReferenceColumn('suppliercompanyid', 'Supplier', 120, $scope.SupplierCompany);
            igrid.AddSimpleColumn('invoiceentryno', 'Entry No', 80);

            if ($scope.mfg_dc2grn) {
                var hasdc = $scope.SearchResult.find(function (item) { return item.invoicetypeid > 0; });
                if (hasdc && hasdc != null) {
                    igrid.AddReferenceColumn('invoicetypeid', 'Type', 80, $scope.InvoiceType);
                }
            }
            igrid.AddSimpleColumn('invoiceno', 'Invoice No', 80);
            igrid.AddFormatedColumn('invoicedate', 'Invoice Date', 90, 'dd/MM/yyyy');
            igrid.AddSimpleColumn('noofbundle', 'Bundles', 80);
            igrid.AddFormatedColumn('baseamount', 'Base', 80, 'N2', 2).total = true;
            igrid.AddFormatedColumn('taxamount', 'Tax', 80, 'N2', 2).total = true;
            igrid.AddFormatedColumn('totalamount', 'Total', 100, 'N2', 2).total = true;



            var commands = "<input type='button' class='btn btn-primary btn-xs' value='Edit'/> &nbsp;"
            commands += "<input type='button' class='btn btn-danger btn-xs' value='Delete'/> &nbsp;"
            commands += "<input type='button' class='btn btn-info btn-xs' value='Print'/>"

            igrid.AddButtonColumn('action', 'ACTION', 180, commands, function (e, args) {
                if ($(e.target).val() == 'Edit')
                    $scope.ManageSearchItem('EditInvoice', args.item);
                else if ($(e.target).val() == 'Delete')
                    $scope.ManageSearchItem('DeleteInvoice', args.item);
                else if ($(e.target).val() == 'Print')
                    $scope.PrintInvoice(args.item.id);
            });

            igrid.EnableTotal();
            igrid.Initialize();
            $scope.igrid = igrid;
            $scope.igrid.UpdateData($scope.SearchResult);

        }



        $scope.findLastPrice = function () {

            var request = {
                "Item": $scope.ItemDetails.Details,
                "securitytoken": "String content"
            }

            request.item.supplierid = $scope.Invoice.supplierid;
            request.item.productid = $scope.ItemDetails.productid;
            request.item.fetchprices = true;
            var srequest = JSON.stringify(request);
            $http({
                method: 'POST',
                url: "PricecontrolService/Search",
                data: srequest
            }).success(function (result) {
                $scope.Price = null;
                $scope.LastPrice = null;
                if (result.items != null && result.items.length > 0) {
                    $scope.Price = result.items[0];
                }

                if (result.lastprice != null) {
                    $scope.LastPrice = result.lastprice;
                }

                $scope.FitSellingPrice();

            }).error(function (result) {
            });
        };

        $scope.egrid = null;
        $scope.ConfigureEditGrid = function () {
            if ($scope.egrid != null)
                $scope.egrid.destroy();

            var igrid = new InfyGrid('grdEdit', '450px', '100%', true, true, true, 70);
            igrid.options.showHeaderRow = true;
            igrid.AddSimpleColumn('sno', 'S.No', 40);

            igrid.AddReferenceColumn('bomcategoryid', 'Category', 100, $scope.BOMCategories);
            igrid.AddReferenceColumn('bomid', 'Component', 100, $scope.BOMAllList);
            column = igrid.AddCustomColumn('bomname', 'Name', 150, function (item, col) {
                if (col.datasource && col.datasource.find) {
                    var bomitem = col.datasource.find(function (sitem) {
                        return sitem.id == item.bomid;
                    });
                    return (bomitem && bomitem != null) ? bomitem.title : '';
                }
                return '';
            });
            column.datasource = $scope.BOMAllList;
            column.enablefilter = true;
            column.getValue = function (item, col) {
                if (col.datasource && col.datasource.find) {
                    var bomitem = col.datasource.find(function (sitem) {
                        return sitem.id == item.bomid;
                    });
                    return (bomitem && bomitem != null) ? bomitem.title : '';
                }
                return '';
            }

            igrid.AddReferenceColumn('colourid', 'Colour', 120, $scope.AllColorList);
            var column = igrid.AddColumn(
                {
                    id: 'bominwardcode', name: 'Barcode', field: 'bominwardcode',
                    width: 50, cssClass: "cell-title", editor: Slick.Editors.Text,
                    'sortable': true
                }
            );
            var column = igrid.AddColumn(
                {
                    id: 'width', name: 'Width', field: 'width',
                    width: 80, cssClass: "cell-title", editor: Slick.Editors.Text,
                    'sortable': true
                }
            );
            igrid.AddFormatedColumn('buyingqty', 'Qty', 70, 'N2f', 2).total = true;
            igrid.AddFormatedColumn('basebuyingprice', 'Rate', 70, 'N2', 2).total = true;
            igrid.AddFormatedColumn('purchasediscount', 'Dscnt', 60, 'N2', 2);
            igrid.AddFormatedColumn('sellingprice', 'Landing', 80, 'N2', 2).total = true;
            igrid.AddFormatedColumn('amount', 'Amount', 80, 'N2', 2).total = true;

            igrid.AddButtonColumn('PO', '', 35, "<button type='button' style='width:30px;height:25px;margin-right:5px;line-height: 0;' value='PO' class='btn btn-primary btn-xs'><i class='fa fa-star-o' style='font:normal normal normal 14px/1 FontAwesome'></i></button>");
            igrid.AddButtonColumn('Edit', '', 35, "<button type='button' style='width:30px;height:25px;margin-right:5px;line-height: 0;' value='Edit' class='btn btn-success btn-xs'><i class='fa fa-pencil' style='font:normal normal normal 14px/1 FontAwesome'></i></button>");
            igrid.AddButtonColumn('Delete', '', 35, "<button type='button' style='width:30px;height:25px;margin-right:5px;line-height: 0;' value='Delete' class='btn btn-danger btn-xs'><i class='fa fa-trash-o' style='font:normal normal normal 14px/1 FontAwesome'></i></button>");

            igrid.EnableTotal();

            igrid.Initialize();
            igrid.dataView.getItemMetadata = function (row) {
                var rowdata = igrid.grid.getData().getItem(row);
                rowdata.style = '';
                if (rowdata.poinfo == undefined ||
                    rowdata.poinfo == null ||
                    rowdata.poinfo.length == 0)
                    rowdata.style = 'igridwithgrey';
                else{
                    var missmatchrate = rowdata.poinfo.find(function (item) {
                        return item.rate != rowdata.basebuyingprice;
                    })
                    if (missmatchrate == undefined ||
                        missmatchrate == null) {
                        rowdata.style = 'igridbigrowswithgreen';
                    }
                    else {
                        rowdata.style = 'igridbigrowswithred';
                    }
                }
                return { 'cssClasses': rowdata.style };
            }

            igrid.grid.onClick.subscribe(function (e) {
                var cell = igrid.grid.getCellFromEvent(e);
                var item = igrid.grid.getDataItem(cell.row);
                var command = igrid.grid.getColumns()[cell.cell].id;
                $scope.$apply(function () {
                    if (command == 'PO')
                        $scope.AllocatePOItems(item, false);
                    else if (command == 'Edit')
                        $scope.ManageSearchItem(command, item);
                    else if (command == 'Delete')
                        $scope.ManageSearchItem(command, item);
                });
            });

            $scope.egrid = igrid;
            $scope.egrid.UpdateData($scope.Invoice.bominvoiceitems);
        }

        $scope.purchasediscountpercentagechanged = function () {
            $scope.ItemDetails.purchasediscount = ($scope.ItemDetails.purchasediscountpercentage * $scope.GetNumber($scope.ItemDetails.basebuyingprice) / 100).round(2);
            $scope.fixTax(false);
        };

        $scope.purchasediscountchanged = function () {
            $scope.ItemDetails.purchasediscountpercentage = ($scope.ItemDetails.purchasediscount / $scope.GetNumber($scope.ItemDetails.basebuyingprice) * 100).round(2);
            $scope.fixTax(false);
        };

        $scope.LastSaved = function () {
            var value = $scope.LocalContext.Get('$scope.BOMInvoice', false);
            if (typeof (value) != 'undefined' && value == null) {
                $rootScope.ShowAlert('No data found', 'No-Data');
                return;
            }
            $scope.Invoice = value;
            $scope.egrid.UpdateData($scope.Invoice.bominvoiceitems);
        };


        if (typeof ($location.search().id) != 'undefined' &&
            $location.search().id.length > 0) {
            request = {
                "Item": { 'lrid': $location.search().id },
                "securitytoken": "String content"
            }
            var srequest = JSON.stringify(request);
            $http({
                method: 'POST',
                url: "BOMinvoiceService/Search",
                data: srequest
            }).success(function (result) {
                $scope.ShowSearch = true;
                if (result.length == 1) {
                    $scope.InvoiceInfo.invoiceid = result[0].id;
                    $scope.InitializeInvoiceList(true);
                }
                else {
                    $scope.SearchResult = result;
                    $scope.igrid.UpdateData($scope.SearchResult);
                }
            }).error(function (result) {
            });
        }

        if (typeof ($location.search().lrid) != 'undefined') {
            $scope.SearchCondition = {
                companyid: 0,
                supplierid: 0,
                invoiceno: "",
                addedfrom: null,
                addedto: null,
                lrid: 0
            }

            $scope.SearchCondition.lrid = $location.search().lrid;
            $scope.Search();
        }


        $scope.ShowComponentCreation = function () {
            var dialog = ngDialog.open({
                template: 'clientside/mfg/views/BOMMaster.html?d=' + (new Date()).getTime(),
                data: [$scope],
                width: '650px',
                controller: 'BOMMasterController',
                className: 'ngdialog-theme-default'
            });
            dialog.closePromise.then(function (data) {
                {
                    if (typeof (data.value) == 'undefined' || data.value == null || data.value == '$document' || data.value == '$escape')
                        return;
                    $scope.BOMAllList.push(data.value);
                    $scope.categoryChange();
                }
            });
        }

        $scope.PrintBarcode = function () {
            var result = [];

            var supplier = $scope.Suppliers.find(function (item) {
                return item.id == $scope.Invoice.supplierid
            });

            $.each($scope.egrid.GetSelectedRows(), function (k, v) {

                var bom = $scope.BOMAllList.find(function (item) {
                    return item.id == v.bomid;
                });

                var colour = bom.colourlist && bom.colourlist != null ? (bom.colourlist.find(function (item) {
                    return item.id == v.colourid;
                })) : { 'name': '' };

                var pcolour = $scope.AllColorList.find(function (item) {
                    return item.id = v.colourid;
                })

                if (typeof (colour) == 'undefined') colour = {};
                if (typeof (pcolour) == 'undefined') pcolour = {};

                result.push({
                    'sno': v.sno, 'id': v.id, 'invoiceid': $scope.Invoice.id, 'colourcode': colour.name, 'suppliername': supplier.name,
                    'colourid': v.colourid, 'bomcode': bom.bommastercode, 'bomname': bom.name, 'bomtitle': bom.title.replace('(' + bom.name + ')', ''), 'suppliercode': supplier.code,
                    'bominwardcode': v.bominwardcode, 'display': bom.name + '/' + colour.name, 'pqty': 1, 'grndate': $scope.Invoice.invoicedate,
                    'grnno': $scope.Invoice.invoiceentryno, 'invoiceno': $scope.Invoice.invoiceno, 'colourname': pcolour.name, 'qty': v.buyingqty
                });
            });
            $scope.ChooseBarcode(result);
        }

        $scope.ChooseBarcode = function (barcodeList) {

            var dialog = ngDialog.open({
                template: 'ChooseBarecodes',
                data: {
                    'restService': $scope.restService,
                    'currentlist': barcodeList
                },
                width: '600px',
                controller: ['$scope', '$timeout', function ($scope, $timeout) {
                    $scope.restService = $scope.ngDialogData.restService;
                    $scope.currentlist = $scope.ngDialogData.currentlist;
                    $scope.SelectAll = function (value) {
                        $.each($scope.currentlist, function (k, v) {
                            v.selected = value;
                        })
                    }

                    $scope.Selected = function () {
                        var returnList = []
                        $.each($scope.currentlist, function (k, v) {
                            if (v.selected) returnList.push(v);
                        });
                        $scope.closeThisDialog(returnList);
                    }

                }],
                className: 'ngdialog-theme-plain'
            });
            dialog.closePromise.then(function (data) {
                {
                    if (typeof (data.value) != 'undefined' && data.value != null && data.value != '$escape'
                        && data.value != '$document') {
                        if (data.value.length > 0) {

                            var requestitem = { 'items': data.value };
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
                                    url: "BOMInvoiceService/PrintBarcode",
                                    data: srequest
                                }).success(function (result) {
                                    if (result != null && result.length > 0) {
                                        $rootScope.Sent2Printer(result.join(';'), printdata);
                                    }
                                });
                            });

                        }
                    }
                }
            });
        }

        $scope.mfg_gatepass = false;
        $scope.mfg_singlepo = false;
        $scope.mfg_dc2grn = false;
        $scope.LoadAppSetting = function () {
            if ($rootScope.UserInfo && $rootScope.UserInfo.appsettings) {

                $scope.mfg_gatepass = false || $rootScope.UserInfo.appsettings.mfg_gatepass;
                $scope.mfg_singlepo = false || $rootScope.UserInfo.appsettings.mfg_singlepo;
                $scope.mfg_dc2grn = false || $rootScope.UserInfo.appsettings.mfg_dc2grn;

                $scope.ConfigureEditGrid();
            } else {
                $timeout(function () {
                    $scope.LoadAppSetting();
                }, 10);
            }
        }

        $timeout(function () {
            $scope.LoadAppSetting();
        }, 10);


    }]);
