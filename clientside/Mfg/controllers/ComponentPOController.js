
var app = angular.module('InfinityLab');

app.controller('ComponentPOController', ['UserDetails', '$scope', '$http', '$location', '$rootScope', '$window', 'restService', 'ngDialog', '$timeout', '$state', 'cntxService', '$stateParams',
    function (UserDetails, $scope, $http, $location, $rootScope, $window, restService, ngDialog, $timeout, $state, cntxService, $stateParams) {
        $scope.Navigation = $rootScope.GetNavigation();
        $rootScope.AccordionName = $location.path();
        $scope.SearchRef = restService.SearchReference;
        $scope.orderqty = 0;
        $scope.orderamount = 0;
        $scope.CurrentBOMImage = null;
        $scope.BOMCategory = restService.Get('ReferencelistService/FULL-BOMCATEGORY', true).then(function (results) {
            $scope.BOMCategory = results;
        });
        $scope.Employee = restService.Get('ReferencelistService/EMPLOY', true).then(function (results) {
            $scope.Employee = results;
        });
        $scope.BOMAllList = restService.Get('ReferencelistService/BOMLIST', true).then(function (results) {
            $scope.BOMAllList = results;
        });

        $scope.SearchBOM = function (value, list) {
            var fitem = list.find(function (item) {
                return item.id == value;
            });
            if (fitem && fitem != null)
                return fitem.name;
            return '-';
        }

        $scope.Company = restService.Get('ReferencelistService/BUYINGCOMPANY', true).then(function (results) {
            $scope.Company = results;
        });

        $scope.DeliveryLocations = restService.Get('ReferencelistService/DELIVERYLOCATION', true).then(function (results) {
            $scope.DeliveryLocations = results;
        });

        $scope.JumpList = [];
        $scope.ColorList = [];
        restService.Get('ReferencelistService/COLOUR', true).then(function (results) {
            $scope.AllColorList = results;
        });

        $scope.Suppliers = restService.Get('ReferencelistService/SUPPLIER', true).then(function (results) {
            $scope.Suppliers = results;
        });
        $scope.LocationList = restService.Get('ReferencelistService/MYLOCATION', true).then(function (results) {
            $scope.LocationList = results;
        });

        $scope.FreightList = [{ id: 1, name: 'To Pay' },
        { id: 2, name: 'Paid' }];

        $scope.SetColourList = function (prditem) {
            if (prditem.colourmode == 2) {
                $scope.ColorList = [];
                $.each(prditem.colourlist, function (k, v) {
                    var colour = $scope.AllColorList.find(function (item) {
                        return item.id == v.id;
                    });
                    if (colour && colour.id > 0) {
                        colour.colourcode = v.name;
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
                    colour.colourcode = prditem.code;
                    colour.imageid = prditem.imageid;
                    $scope.ColorList.push(colour);
                }
            }
            else {
                $scope.ColorList = [];
                $scope.ProductRequest.Details.colourcode = prditem.code;
                $scope.ProductRequest.Details.imageid = prditem.imageid;
            }
        }

        $scope.ColourChanged = function () {
            $scope.ProductRequest.Details.colourcode = '';
            $scope.ProductRequest.Details.imageid = '';
            if ($scope.ProductRequest.Details.colourid &&
                $scope.ProductRequest.Details.colourid > 0) {
                var colour = $scope.ColorList.find(function (item) {
                    return item.id == $scope.ProductRequest.Details.colourid;
                });
                if (colour && colour != null) {
                    $scope.ProductRequest.Details.colourcode = colour.colourcode;
                    $scope.ProductRequest.Details.imageid = colour.imageid;
                }
            }
        }

        $scope.bomchange = function (onlyimage, callback) {
            $scope.CurrentBOMImage = null;
            if (!onlyimage) {
                var bom = $scope.BOMAllList.find(function (item) {
                    return item.code == $scope.ProductRequest.Details.bomcode;
                });

                if (typeof (bom) != 'undefined' && bom != null) {
                    $scope.ProductRequest.Details.bomid = bom.id;
                    $scope.ProductRequest.Details.width = bom.width;
                    $scope.ProductRequest.Details.rate = bom.buyingrate;
                    $scope.ProductRequest.Details.designid = bom.designno;
                    $scope.ProductRequest.Details.brandname = bom.supplierbrand;
                    $scope.CurrentBOMImage = bom.imageid;
                    $scope.SetColourList(bom);
                    if (callback)
                        $timeout(callback, 20);
                }
            }
            else {
                var bom = $scope.BOMAllList.find(function (item) {
                    return item.id == $scope.ProductRequest.Details.bomid;
                });
                if (typeof (bom) != 'undefined' && bom != null) {
                    $scope.ProductRequest.Details.bomid = bom.id;
                    $scope.ProductRequest.Details.width = bom.width;
                    $scope.ProductRequest.Details.rate = bom.buyingrate;
                    $scope.CurrentBOMImage = bom.imageid;
                    $scope.SetColourList(bom);
                    if (callback)
                        $timeout(callback, 20);
                }
            }
        }

        $scope.poimageuploaded = function (imageid, scope) {
            $scope.ProductRequest.formimageid = imageid[0];
            $scope.ProductRequest.timageid = null;
        }

        $scope.imageuploaded = function (imageid, scope) {
            scope.item.imageid = null;
            scope.item.timageid = null;
            $rootScope.CropImage(imageid, 'IMAGE', function (data) {
                scope.item.imageid = data;
            }, null, { 'width': 200, 'height': 100 });
        }

        $scope.searchcategoryChange = function () {
            var BOMList = [];
            $.each($scope.BOMAllList, function (k, v) {
                if (v.categoryid == $scope.SearchCondition.bomcategoryid)
                    BOMList.push(v);
            });
            $scope.BOMList = BOMList;
        }

        $scope.AutoBOMCode = false;
        $scope.categoryChange = function (focus, callback) {

            if (typeof ($scope.ProductRequest.Details.bomcategoryid) != 'undefined' &&
                $scope.ProductRequest.Details.bomcategoryid != null) {
                var bomcagtegory = $scope.BOMCategory.find(function (item) {
                    return item.id == $scope.ProductRequest.Details.bomcategoryid;
                });

                if (typeof (bomcagtegory) != 'undefined' && bomcagtegory != null) {
                    if (bomcagtegory.additionalinfo.autocreate) {
                        $scope.AutoBOMCode = true;
                        $scope.ProductRequest.Details.bomid = 0;
                        $scope.ProductRequest.Details.taxid = bomcagtegory.additionalinfo.taxid;
                        $scope.ProductRequest.Details.uomid = bomcagtegory.additionalinfo.uomid;
                        $scope.ProductRequest.Details.coretypeid = bomcagtegory.additionalinfo.parentid;
                        if (focus)
                            $scope.setFocusautobomcode();
                    }
                    else {
                        $scope.AutoBOMCode = false;
                        $scope.ProductRequest.Details.bomcode = '';
                        var BOMList = [];
                        $.each($scope.BOMAllList, function (k, v) {
                            if (v.categoryid == $scope.ProductRequest.Details.bomcategoryid)
                                BOMList.push(v);
                        });
                        $scope.BOMList = BOMList;
                        if (focus) {
                            $timeout(function () {
                                $scope.setFocusbomid();
                            }, 20);
                        }
                        if (callback)
                            $timeout(callback, 20);
                    }
                }
            }
        }


        $scope.Agent = restService.Get('ReferencelistService/AGENT', true).then(function (results) {
            $scope.Agent = results;
        });
        $scope.ShowStockView = false;

        $scope.POMode = [{ id: 0, name: 'Size Range Based' },
        { id: 1, name: 'Size Set Based' },
        { id: 2, name: 'Single Size' }];

        $scope.FreightList = [{ id: 1, name: 'To Pay' },
        { id: 2, name: 'Paid' }];

        $scope.POStatus = [{ id: 0, name: 'PENDING' }, { id: 1, name: 'APPROVED' }, { id: 2, name: 'REJECTED' }, { id: 3, name: 'CANCELLED' }, { id: 4, name: 'COMPLETED' }];
        $scope.POSearchStatus = [{ id: 0, name: 'PENDING APPROVAL' }, { id: 1, name: 'APPROVED' }, { id: 2, name: 'REJECTED' }, { id: 3, name: 'CANCELLED' }, { id: 4, name: 'COMPLETED' }, { id: -9, name: 'ALL' }];
        $scope.POList = [];
        $scope.PendingPOList = [];
        $scope.Entity = {
            "id": 0,
            "expectedate": new Date(),
            "podate": new Date(),
            "supplierid": null,
            "locationid": 0,
            "deliverylocationid": 0,
            "pono": "",
            "termsandcondition": {},
            "status": 0,
            "isactive": true,
            "createdby": 0,
            "modifiedby": 0,
            "version": 1,
            "createdon": "",
            "modifiedon": "",
            "tolerance": 5,
            "preparedby": 0,
            "approvedby": 0,
            "ComponentPOitems": [],
            Details: {
                "oqty": 0,
                "bomcategoryid": null,
                "bomid": null,
                "colourid": null,
                "width": "",
                "id": 0,
                "qty": "",
                "rate": "",
                "amount": "",
            }
        };
        $scope.ProductRequest = (JSON.parse(JSON.stringify($scope.Entity)));
        $scope.SearchCondition = {
            'pono': null,
            'supplierid': 0,
            'expectedate': null,
            'statusid': -9
        };

        $scope.ErrorStyle = "success";
        $scope.ShowSearch = false;
        $scope.SearchResult = [];

        $scope.ErrorMessage = "";

        $scope.Clear = function () {
            $scope.ProductRequest = (JSON.parse(JSON.stringify($scope.Entity)));
        };

        $scope.Save = function () {
            if ($scope.ProductRequest.companyid == null || $scope.ProductRequest.companyid == 0) {
                $rootScope.Toaster.Error("Please select company!");
                return;
            }
            if ($scope.ProductRequest.supplierid == null || $scope.ProductRequest.supplierid == 0) {
                $rootScope.Toaster.Error("Please select supplier!");
                return;
            }
            if ($scope.ProductRequest.podate == null || $scope.ProductRequest.podate == 0) {
                $rootScope.Toaster.Error("Please select PO date!");
                return;
            }

            if ($scope.SaveRequest) return;
            $scope.SaveRequest = true;
            var request = {
                "item": $scope.ProductRequest,
                "securitytoken": "String content"
            }

            var srequest = JSON.stringify(request);

            $http({
                method: 'POST',
                url: "ComponentPOService/Save",
                data: srequest
            }).success(function (result) {
                $scope.SaveRequest = false;
                if (result != null && result.id > 0) {
                    $rootScope.Toaster.Info("Successfully saved with PO No :" + result.pono);
                    $scope.ProductRequest.pono = result.pono;
                    $scope.ProductRequest.id = result.id;
                    $scope.ProductRequest.ComponentPOitems = result.ComponentPOitems;
                }
                else {
                    $rootScope.Toaster.Error("Attempt Failed." + result.ErrorMessage);
                    $scope.ErrorMessage = result.ErrorMessage;
                    $scope.ErrorStyle = "danger";
                }
                $scope.CalculateOrderQty();
            }).error(function (result) {
                $scope.SaveRequest = false;
            });
        }

        $scope.ChangeOrderStatus = function () {
            if ($scope.ProductRequest.id == null || $scope.ProductRequest.id == 0) {
                $rootScope.Toaster.Error("Please select purchase order!");
                return;
            }

            var request = {
                "item": $scope.ProductRequest,
                "securitytoken": "String content"
            }

            var srequest = JSON.stringify(request);

            $http({
                method: 'POST',
                url: "ComponentPOService/UpdateStatus",
                data: srequest
            }).success(function (result) {
                $rootScope.Toaster.Info("Status updated successfully");

            }).error(function (result) {
            });

        }

        $scope.AddItem = function () {
            if ($scope.ProductRequest.companyid == null || $scope.ProductRequest.companyid == 0) {
                $scope.Toaster.Info("Please enter company");
                return;
            }

            if ($scope.ProductRequest.supplierid == null || $scope.ProductRequest.supplierid == 0) {
                $scope.Toaster.Info("Please enter order location");
                return;
            }

            if ($scope.ProductRequest.Details.bomcategoryid == null || $scope.ProductRequest.Details.bomcategoryid == 0) {
                $scope.Toaster.Info("Please Select BOM Category");
                return;
            }

            item = {
                'bomcategoryid': $scope.ProductRequest.Details.bomcategoryid,
                'bomcode': $scope.AutoBOMCode ? $scope.ProductRequest.Details.bomcode : '',
                'bomid': $scope.AutoBOMCode ? 0 : $scope.ProductRequest.Details.bomid,
                'colourcode': $scope.ProductRequest.Details.colourcode,
                'colourshade': $scope.ProductRequest.Details.colourshade,
                'qty': $scope.ProductRequest.Details.qty,
                'taxid': $scope.ProductRequest.Details.taxid,
                'brandname': $scope.ProductRequest.Details.brandname,
                'designid': $scope.ProductRequest.Details.designid,
                'uomid': $scope.ProductRequest.Details.uomid,
                'coretypeid': $scope.ProductRequest.Details.coretypeid,
                'isactive': true,
                'imageid': $scope.CurrentBOMImage,
                'version': 1,
                'colourid': $scope.ProductRequest.Details.colourid,
                'width': $scope.ProductRequest.Details.width,
                'rate': $scope.ProductRequest.Details.rate,
                'amount': 0,
                'id': 0
            };

            if (typeof (item.imageid) != 'undefined' && item.imageid != null &&
                item.imageid.length > 0)
                $scope.ProductRequest.ShowImage = true;

            item.amount = item.rate * item.qty;
            $scope.ProductRequest.ComponentPOitems.push(item);
            $scope.ProductRequest.Details.id = 0;
            $scope.ProductRequest.Details.imageid = '';
            $scope.ProductRequest.Details.bomcode = '',
            $scope.ProductRequest.Details.colourcode = '';
            $scope.ProductRequest.Details.colourshade = '';
            $scope.ProductRequest.Details.qty = '';
            $scope.ProductRequest.Details.brandname = '';
            $scope.ProductRequest.Details.designid = '';
            $scope.CurrentBOMImage = null;
            $scope.ProductRequest.Details.colourid = null;
            $scope.ProductRequest.Details.width = '';
            $scope.ProductRequest.Details.rate = '';
            $scope.CalculateOrderQty();
            $scope.setFocusbomcategoryid();
        };

        $scope.CalculateOrderQty = function () {
            $scope.orderqty = 0;
            $scope.orderamount = 0;
            $.each($scope.ProductRequest.ComponentPOitems, function (k, v) {
                if ($rootScope.GetNumber(v.qty) == 'undefined' || $rootScope.GetNumber(v.qty) == 0)
                    v.qty = 0;
                $scope.orderqty += $rootScope.GetNumber(v.qty);
                $scope.orderamount += $rootScope.GetNumber(v.qty) * $rootScope.GetNumber(v.rate);
            });
        };


        $scope.ManageItem = function (type, item) {
            if (type == 'Edit') {
                if (typeof ($scope.ProductRequest) == 'undefined')
                    $scope.ProductRequest = {};
                if (typeof ($scope.ProductRequest.Details) == 'undefined')
                    $scope.ProductRequest.Details = {};
                $scope.ProductRequest.Details.bomcategoryid = item.bomcategoryid;
                $scope.categoryChange(false, function () {

                    $scope.ProductRequest.Details.bomcode = item.bomcode;
                    $scope.ProductRequest.Details.bomtypeid = item.bomtypeid;
                    $scope.ProductRequest.Details.coretypeid = item.coretypeid;
                    $scope.ProductRequest.Details.bomid = item.bomid;
                    $scope.bomchange(true, function () {
                        $scope.ProductRequest.Details.taxid = item.taxid;
                        $scope.ProductRequest.Details.uomid = item.uomid;
                        $scope.ProductRequest.Details.qty = item.qty;
                        $scope.ProductRequest.Details.rate = item.rate;
                        $scope.ProductRequest.Details.amount = item.amount;
                        $scope.ProductRequest.Details.colourid = item.colourid;

                        $scope.ProductRequest.Details.brandname = item.brandname;
                        $scope.ProductRequest.Details.designid = item.designid;
                        $scope.ProductRequest.Details.colourcode = item.colourcode;

                        $scope.ColourChanged();
                        $scope.ProductRequest.Details.width = item.width;
                        $scope.CurrentBOMImage = item.imageid;
                        $scope.ProductRequest.Details.bomid = null;
                        $scope.ProductRequest.Details.colourid = null;
                        $scope.ProductRequest.ComponentPOitems.splice($scope.ProductRequest.ComponentPOitems.indexOf(item), 1);

                        $timeout(function () {
                            $scope.ProductRequest.Details.bomid = item.bomid;
                            $scope.ProductRequest.Details.colourid = item.colourid;
                            $scope.CurrentBOMImage = item.imageid;
                        }, 10);
                        $scope.CalculateOrderQty();

                    });
                });


            } else if (type == 'Delete') {
                if (item.rqty > 0) {
                    $rootScope.Toaster.Error("Item Can't be deleted!! Item already received from the supplier.")
                    return;
                }
                else
                    $scope.ProductRequest.ComponentPOitems.splice($scope.ProductRequest.ComponentPOitems.indexOf(item), 1);
                $scope.CalculateOrderQty();
            }
        };

        $scope.ManageSearchItem = function (command, item) {
            if (command == 'Edit') {
                $scope.ProductRequest = item;
                var request = {
                    "Item": {
                        'componentorderid': item.id
                    },
                    "securitytoken": "String content"
                }
                var srequest = JSON.stringify(request);
                $http({
                    method: 'POST',
                    url: "ComponentPOitemsService/Search",
                    data: srequest
                }).success(function (result) {
                    if (result.length > 0)
                        $scope.ProductRequest.ComponentPOitems = result;
                    $scope.ShowSearch = false;
                    $scope.Navigation.SetMode('EDIT');
                    $scope.CalculateOrderQty();
                });

            }
            else if (command == "Delete") {
                $.confirm({
                    title: 'Delete',
                    content: 'Do you want to delete ?',
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
                                    url: "ComponentPOService/Delete",
                                    data: srequest
                                }).success(function (result) {

                                    if (result) {
                                        $rootScope.Toaster.Error("Successfully deleted");
                                        //$scope.ErrorMessage = "Successfully deleted";
                                        $scope.SearchResult.splice($scope.SearchResult.indexOf(item), 1);
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
            else if (command == "Cancel") {
                $.confirm({
                    title: 'Cancel',
                    content: 'Do you want to cancel pending items on this PO?',
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
                                    url: "ComponentPOService/Cancel",
                                    data: srequest
                                }).success(function (result) {

                                    if (result) {
                                        $rootScope.Toaster.Error("Successfully Cancel");
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
            else if (command == "Complete") {
                $.confirm({
                    title: 'Complete',
                    content: 'Do you want to Clear pending items on this PO?',
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
                                    url: "ComponentPOService/Complete",
                                    data: srequest
                                }).success(function (result) {

                                    if (result) {
                                        $rootScope.Toaster.Error("Successfully Cancel");
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
        };

        $scope.qtyChanged = function () {
            $scope.ProductRequest.Details.oqty = 0;
            $.each($scope.JumpList, function (k, v) {
                if ($rootScope.GetNumber(v.qty) == 'undefined' || $rootScope.GetNumber(v.qty) == 0)
                    $scope.ProductRequest.Details.oqty += 0;
                else
                    $scope.ProductRequest.Details.oqty += $rootScope.GetNumber(v.qty);
            });
        }

        $scope.CheckSize = function () {

            if (typeof ($scope.ProductRequest.Details.size) != 'undefined' &&
                $scope.ProductRequest.Details.size != null && $scope.ProductRequest.mode == 0) {
                $scope.GetJUMPInformation($scope.ProductRequest.Details.size);
            }

            //$rootScope.Prompt('Jump', 'Jump (start-increment-end)', function (count) {
            //});
        }

        $scope.GetJUMPInformation = function (count) {

            if (count.length > 0) {

                var sizeidList = count.split(',');
                var sizeList = sizeidList[0].split('-');
                if (sizeList.length == 2 && /^[a-zA-Z2-6]*$/.test(sizeList[0])) {

                    var sizeIndex = '3XS,XXS,XS,S,M,L,XL,XXL,XXXL,4XL,5XL,6XL,7XL'.split(',');
                    var startsize = sizeIndex.indexOf(sizeList[0]);
                    var endsize = sizeIndex.indexOf(sizeList[1]);
                    if (startsize < 0 || endsize < 0) {
                        sizeIndex = '3XS,XXS,XS,S,M,L,XL,2XL,3XL,4XL,5XL,6XL,7XL'.split(',');
                        startsize = sizeIndex.indexOf(sizelist[0]);
                        endsize = sizeIndex.indexOf(sizelist[1]);
                    }
                    sizeList = [];
                    for (var index = startsize; index <= endsize; index++) {
                        sizeList.push(sizeIndex[index]);
                    }

                }
                else if (sizeList.length == 3) {

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
                $.each($scope.SizeList, function (k, v) {
                    $.each(sizeList, function (kk, vv) {
                        if (vv == v.name) {
                            if (idList.find(function (item) {
                                return item.name == v.name;
                            }) == null) {
                                idList.push({ 'id': v.id, 'name': v.name, 'qty': 0, 'index': kk });
                            }
                        }
                    });
                });

                idList.sort(function (a, b) {
                    return a.index - b.index;
                });

                $scope.JumpList = idList;
                //$scope.ProductRequest.Details.qty = 0;
                $('#jumplist INPUT').first().focus();
            }
        }

        $scope.SearchItem = function () {

            if ($scope.ProductRequest.Details.productid == null || $scope.ProductRequest.Details.productid == 0) {
                $scope.Toaster.Info("Please select product");
                return;
            }

            $scope.ProductRequest.Details.id = 0;
            $scope.ProductRequest.Details.brandname = $scope.SearchRef($scope.ProductRequest.Details.brandid, $scope.BrandName, '');
            $scope.ProductRequest.Details.printingname = $scope.SearchRef($scope.ProductRequest.Details.productid, $scope.ProductList, '');
            $scope.ProductRequest.Details.patternname = $scope.SearchRef($scope.ProductRequest.Details.patternid, $scope.PatternList, '');
            $scope.ProductRequest.Details.materialname = $scope.SearchRef($scope.ProductRequest.Details.materialid, $scope.MaterialList, '');
            $scope.ProductRequest.Details.colourname = $scope.SearchRef($scope.ProductRequest.Details.colourid, $scope.ColorList, '');
            $scope.ProductRequest.Details.sizename = $scope.SearchRef($scope.ProductRequest.Details.sizeid, $scope.SizeList, '');
            $scope.ProductRequest.Details.stylename = $scope.SearchRef($scope.ProductRequest.Details.styleid, $scope.StyleList, '');
            $scope.ProductRequest.Details.typename = $scope.SearchRef($scope.ProductRequest.Details.typeid, $scope.TypeList, '');
            $scope.ProductRequest.Details.autocreate = true;

            var request = {
                "Item": $scope.ProductRequest.Details,
                "securitytoken": "String content"
            }
            var srequest = JSON.stringify(request);
            $http({
                method: 'POST',
                url: "ItemsService/Search",
                data: srequest
            }).success(function (result) {
                if (result != null && result.length == 0) {
                    $rootScope.Toaster.Info("No Item found. Please create the item");
                    return;
                }
                if (result.length == 1) {
                    $scope.ProductRequest.Details = result[0];
                    $scope.ProductRequest.Actual = (JSON.parse(JSON.stringify($scope.ProductRequest.Details)));
                }
                else if (result.length > 1)
                    $scope.ShowProductSearch(result);
            });
        };

        $scope.SearchItemByName = function () {
            var request = {
                "Item": {
                    'name': $scope.ProductRequest.Details.printingname, 'productid': $scope.ProductRequest.Details.productid
                },
                "securitytoken": "String content"
            }

            var srequest = JSON.stringify(request);
            $http({
                method: 'POST',
                url: "ItemsService/SearchByName",
                data: srequest
            }).success(function (result) {
                if (result.length == 1) {
                    $scope.ProductRequest.Details = result[0];
                    $scope.ProductRequest.Actual = (JSON.parse(JSON.stringify($scope.ProductRequest.Details)));
                }
                else if (result.length > 1)
                    $scope.ShowProductSearch(result);
            });
        };

        $scope.Export = function () {
            if (typeof ($scope.igrid) == 'undefined') return;
            $scope.exportHref = $scope.igrid.ExportExcel($scope.igrid.grid, 'Product List');
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
        $scope.Search = function () {
            var request = {
                "Item": $scope.SearchCondition,
                "securitytoken": "String content"
            }

            var srequest = JSON.stringify(request);
            $http({
                method: 'POST',
                url: "ComponentPOService/Search",
                data: srequest
            }).success(function (result) {
                $scope.SearchResult = result;
            });
        };

        $scope.Publish = function () {
            restService.Get('ComponentPOService/Publish/' + $scope.UserInfo.OrganizationID + '/' + $scope.ProductRequest.id, true).then(function (results) {
                if (results != null && results != 'undefined' && results == 'true')
                    $rootScope.Toaster.Info("Purchase Order Sent Successfully.")
                else
                    $rootScope.Toaster.Info("Error in Sending Purchase Order.")
            });
        }

        $scope.exporttoExcel = function (infygrid, filename, tableid) {
            var title = $rootScope.UserInfo.CompanyName;
            var subtitle = ''
            if ($scope.SearchCondition.companyid != null && $scope.SearchCondition.companyid > 0)
                title = $scope.SearchRef($scope.SearchCondition.companyid, $scope.Company);

            if (tableid == 'posummary')
                subtitle = 'PURCHASE ORDER - SUMMARY REPORT';
            else if (tableid == 'podetails')
                subtitle = 'PURCHASE ORDER - ITEM DETAILS REPORT';
            //subtitle = 'PURCHASE ORDER - SUMMARY REPORT FROM : ' + $filter('date')($scope.SearchCondition.fromdate, 'dd-MMM-yyyy') + " TO " + $filter('date')($scope.SearchCondition.todate, 'dd-MMM-yyyy') + " ( " + $scope.SearchRef($scope.SearchCondition.companyid, $scope.Company) + " ) ";

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

        $scope.Print = function (id) {
            $timeout(function () {
                $rootScope.Download('ComponentPOService/PrintPDF/' + $scope.UserInfo.OrganizationID + '/' + (typeof (id) == 'undefined' ? $scope.ProductRequest.id : id), 'po.pdf', 'application/pdf');
                return;

                var link = document.createElement('a');
                link.download = "po.pdf";
                link.href = 'ComponentPOService/PrintPDF/' + $scope.UserInfo.OrganizationID + '/' + $scope.ProductRequest.id;
                document.body.appendChild(link);
                link.click();
                $timeout(function () {
                    link.remove();
                }, 100);
            }, 10);
        }


        $scope.ShowComponentCreation = function () {
            var dialog = ngDialog.open({
                template: 'clientside/mfg/views/BOMMaster.html?d=' + (new Date()).getTime(),
                data: [$scope],
                width: '800px',
                controller: 'BOMMasterController',
                className: 'ngdialog-theme-default'
            });
            dialog.closePromise.then(function (data) {
                {
                    if (typeof (data.value) == 'undefined' || data.value == null || data.value == '$document' || data.value == '$escape')
                        return;
                    $scope.BOMAllList.push(data.value);
                    $scope.categoryChange(true);
                }
            });
        }

        $scope.POConfig = function () {
            if ($scope.igrid != null)
                $scope.igrid.destroy();

            var igrid = new InfyGrid('gridContainer', '400px', '100%', true, true, true, 0);
            igrid.options.showHeaderRow = true;

            igrid.AddSimpleColumn('pono', 'Order No', 80);
            igrid.AddFormatedColumn('podate', 'Order Date', 100, 'dd-MM-yyyy');
            igrid.AddReferenceColumn('supplierid', 'Supplier Name', 150, $scope.Suppliers);
            igrid.AddReferenceColumn('locationid', 'Order Location', 100, $scope.LocationList);
            igrid.AddReferenceColumn('deliverylocationid', 'Delivery Location', 100, $scope.LocationList);
            igrid.AddFormatedColumn('expectedate', 'Due Date', 100, 'dd-MM-yyyy');
            igrid.AddSimpleColumn('tolerance', 'Qty Tolerance %', 100);
            igrid.AddReferenceColumn('agentid', 'Agent Name', 100, $scope.Agent);
            igrid.AddReferenceColumn('status', 'PO Status', 100, $scope.POStatus);
            igrid.AddFormatedColumn('ordered', 'Ordered Qty', 100, 'N2', 2).total = true;
            igrid.AddFormatedColumn('received', 'Received Qty', 100, 'N2', 2).total = true;
            igrid.AddFormatedColumn('cancelled', 'Cancelled Qty', 100, 'N2', 2).total = true;

            igrid.AddButtonColumn('action', 'Action', 50, 'View', function (e, args) {

                $scope.$apply(function () {
                    $scope.ShowOrderItems('VIEW', args.item);
                });
                return true;

            });
            igrid.EnableTotal();
            igrid.Initialize();
            $scope.igrid = igrid;

            $scope.igrid.UpdateData($scope.POList);


        }

        $timeout(function () {
            if (typeof ($location.search().id) != 'undefined' &&
                $location.search().id.length > 0) {
                request = {
                    "Item": { 'id': $location.search().id },
                    "SecurityToken": "String content"
                }

                var srequest = JSON.stringify(request);

                $http({
                    method: 'POST',
                    url: "ComponentPOService/Search",
                    data: srequest
                }).success(function (result) {
                    $scope.ShowSearch = true;
                    if (angular.isArray(result)) {

                        $scope.ProductRequest = result[0];
                        var request = {
                            "Item": {
                                'componentorderid': result[0]
                            },
                            "securitytoken": "String content"
                        }
                        var srequest = JSON.stringify(request);
                        $http({
                            method: 'POST',
                            url: "ComponentPOitemsService/Search",
                            data: srequest
                        }).success(function (result) {
                            if (result.length > 0)
                                $scope.ProductRequest.ComponentPOitems = result;
                            $scope.ShowSearch = false;
                            $scope.Navigation.SetMode('EDIT');
                            $scope.CalculateOrderQty();
                        });

                    }
                }).error(function (result) {
                });
            }
        }, 100);
    }]);
