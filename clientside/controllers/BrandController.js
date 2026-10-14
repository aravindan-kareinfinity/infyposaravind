
var app = angular.module('InfinityLab');

app.controller('BrandController', ['UserDetails', '$scope', '$http', '$location', '$rootScope', '$window', 'restService', '$timeout','ngDialog',
    function (UserDetails, $scope, $http, $location, $rootScope, $window, restService, $timeout, ngDialog) {
        $scope.Navigation = $rootScope.GetNavigation();
        $scope.SearchRef = restService.SearchReference;
        $scope.selectedcount = 0;
        $scope.Products = restService.Get('ReferencelistService/PRODUCTS', true).then(function (results) {
            $scope.Products = results;
            results.push({ id: -1, name: 'All Products' });
        });

        $scope.Discount = [{ 'id': 1, 'name': 'No Discount' }, { 'id': 2, 'name': 'Discount By Amount' }, { 'id': 3, 'name': 'Discount By Percentage' }];

        $scope.SizeGroup = [{ 'id': 1, 'name': 'Kids' }, { 'id': 2, 'name': 'Shirts' }, { 'id': 3, 'name': 'Pants' }, { 'id': 4, 'name': 'Inners' }, { 'id': 5, 'name': 'Material' }, { 'id': 6, 'name': 'none' }];

        $scope.CoreBrand = [{ 'id': 1, 'name': 'Pure Brand' }, { 'id': 2, 'name': 'Local Brand' }, { 'id': 3, 'name': 'Trial Brand' }, {'id':4, 'name': 'Un Brand'}, {'id':5, 'name':'none'}];


        $scope.Entity = {
            "code": "",
            "name": "",
            "printingname": "",
            "barcodeprefix": "",
            "barcodesuffix": "",
            "discountmode": 0,
            "discountvalue": 0,
            "preferredsupplierid": 0,
            "corebrand": 0,
            "isactive": true,
            "createdby": 0,
            "modifiedby": 0,
            "version": 1,
            "createdon": "0001-01-01T00:00:00",
            "modifiedon": "0001-01-01T00:00:00",
            "marginlist":[]
        };

        $scope.Brand = (JSON.parse(JSON.stringify($scope.Entity)));

        $scope.ErrorStyle = "success";
        $scope.ShowSearch = false;
        $scope.SearchResult = [];

        $scope.SwitchPanel = function () {
            $scope.ShowSearch = !$scope.ShowSearch;
            $scope.Brand = (JSON.parse(JSON.stringify($scope.Entity)));
        }

        $scope.ErrorMessage = "";

        $scope.AddMargin = function () {
            if ($scope.Brand.marginlist == undefined || $scope.Brand.marginlist == null) {
                $scope.Brand.marginlist = [];
            }
            $scope.Brand.marginlist.push({'productid':$scope.productid,'margin':$scope.margin});
        }

        $scope.RemoveMargin = function (item) {
            $scope.Brand.marginlist.splice($scope.Brand.marginlist.indexOf(item), 1);
        }

        $scope.SearchCondition = {
            code: "",
            name: ""
        }

        $scope.New = function () {
            $scope.Brand = (JSON.parse(JSON.stringify($scope.Entity)));
            $scope.ErrorMessage = "";
        }

        $scope.imageuploaded = function (value) {
            $scope.Brand.logoid = value[0];
            $scope.Brand.timageid = null;
        }

        $scope.Save = function () {

            if ($scope.Brand.code == null || $scope.Brand.code == 0) {
                $scope['setFocusfirst']();
                $rootScope.Toaster.Error("Please select Code");
                return;
            }

            if ($scope.Brand.name == null || $scope.Brand.name == 0) {
                $scope['setFocusname']();
                $rootScope.Toaster.Error("Please select Name");
                return;
            }

            if ($scope.Brand.printingname == null || $scope.Brand.printingname == 0) {
                $scope['setFocusprintingname']();
                $rootScope.Toaster.Error("Please select Printing Name");
                return;
            }

            //$scope.Brand.patterns = { items: [] };
            //$.each($scope.patterns, function (k, v) {
            //    if (v.selected)
            //        $scope.Brand.patterns.items.push({ 'name': v.name, 'id': v.id, 'available': true, 'ro':v.ro });
            //});
            //$scope.Brand.sizes = { items: [] };
            //$.each($scope.sizes, function (k, v) {
            //    if (v.selected)
            //        $scope.Brand.sizes.items.push({ 'name': v.name, 'id': v.id, 'available': true, 'ro':v.ro });
            //});
            //$scope.Brand.styles = { items: [] };
            //$.each($scope.styles, function (k, v) {
            //    if (v.selected)
            //        $scope.Brand.styles.items.push({ 'name': v.name, 'id': v.id, 'available': true, 'ro':v.ro });
            //});
            //$scope.Brand.materials = { items: [] };
            //$.each($scope.materials, function (k, v) {
            //    if (v.selected)
            //        $scope.Brand.materials.items.push({ 'name': v.name, 'id': v.id, 'available': true, 'ro':v.ro });
            //});

            //$scope.Brand.colours = { items: [] };
            //$.each($scope.ColorList, function (k, v) {
            //    if (v.selected)
            //        $scope.Brand.colours.items.push({ 'name': v.name, 'id': v.id, 'available': true, 'ro':v.ro });
            //});
            request = {
                "Item": $scope.Brand,
                "securitytoken": "String content"
            }

            var srequest = JSON.stringify(request);

            $http({
                method: 'POST',
                url: "BrandService/Save",
                data: srequest
            }).success(function (result) {

                if (result != null && result.id > 0) {

                    $rootScope.Toaster.Info("Brand Saved Successfully.!");
                    $scope.Brand = (JSON.parse(JSON.stringify($scope.Entity)));
                }
                else {

                    $rootScope.Toaster.Error(result.ErrorMessage);

                }

            }).error(function (result) {
            });

        }

        $scope.Export = function () {
            if (typeof ($scope.igrid) == 'undefined') return;
            $scope.exportHref = $scope.igrid.ExportExcel($scope.igrid.grid, 'Brand List');
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

        $scope.MergeSelected = function () {
            var dialog = ngDialog.open({
                template: 'ChooseMerge',
                data: {
                    'rows': $scope.igrid.GetSelectedRows(),
                    'scope': $scope
                },
                width: '500px',
                controller: ['$scope', '$timeout', function ($scope, $timeout) {
                    $scope.SelectedRows = (JSON.parse(JSON.stringify($scope.ngDialogData.rows)));
                    $scope.SelectedItem = null;
                    $timeout(function () {
                        $scope.id = 0;
                    }, 10);
                    $scope.rowselected = function () {
                        $scope.SelectedRows = (JSON.parse(JSON.stringify($scope.ngDialogData.rows)));
                        if (typeof ($scope.id) != 'undefined' && $scope.id > 0) {
                            var selected = $scope.SelectedRows.find(function (item) {
                                return item.id == $scope.id;
                            });
                            $scope.SelectedItem = selected;
                        }
                    }
                    $scope.Selected = function () {
                        $scope.closeThisDialog({ 'selectedid': $scope.id });
                    };
                }],
                className: 'ngdialog-theme-plain'
            });
            dialog.closePromise.then(function (data) {
                {
                    if (typeof (data.value) != 'undefined' && data.value != null
                        && data.value != '$escape' && data.value != '$document') {
                        request = {
                            "item": {
                                'id': data.value.selectedid,
                                'deletelist': $scope.igrid.GetSelectedRows()
                            },
                            "securitytoken": "String content"
                        }
                        var srequest = JSON.stringify(request);
                        $http({
                            method: 'POST',
                            url: "BrandService/Merge",
                            data: srequest
                        }).success(function (result) {
                            if (result) {
                                var rows = $scope.igrid.GetSelectedRows();
                                $.each(rows, function (k, v) {
                                    if (v.id != data.value.selectedid) {
                                        $scope.SearchResult.splice($scope.SearchResult.indexOf(v), 1);
                                    }
                                });
                                $scope.igrid.UpdateData($scope.SearchResult);
                            }
                        });
                    }
                }
            });
        }

        $scope.ManageSearchItem = function (action, item) {
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
                                // var srequest = JSON.stringify(request);
                                $http({
                                    method: 'POST',
                                    url: "BrandService/Delete",
                                    data: request
                                }).success(function (result) {

                                    if (result) {
                                        $scope.ErrorMessage = "Successfully deleted";
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
                $scope.Navigation.SetMode('EDIT');
                $scope.ShowSearch = false;
                $scope.Brand = item;
            }
        }


        
        $scope.Search = function () {

            request = {
                "Item": $scope.SearchCondition,
                "securitytoken": "String content"
            }

            //    var srequest = JSON.stringify(request);

            $http({
                method: 'POST',
                url: "BrandService/Search",
                data: request
            }).success(function (result) {

                $scope.SearchResult = result;
                $scope.BrandConfig();
            }).error(function (result) {
            });
        }

        $scope.BrandConfig = function () {
            if ($scope.igrid != null)
                $scope.igrid.destroy();

            var igrid = new InfyGrid('gridContainer', '450px', '100%', true, true, true,0);
            igrid.options.showHeaderRow = true;

            igrid.AddSimpleColumn('code', 'Code', 100);
            igrid.AddSimpleColumn('name', 'Name', 200);
            igrid.AddSimpleColumn('printingname', 'Printing Name', 150);
            igrid.AddSimpleColumn('barcodeprefix', 'Barcode Prefix', 100);
            igrid.AddSimpleColumn('barcodesuffix', 'Barcode Suffix', 100);
            igrid.AddReferenceColumn('preferredsupplierid', 'Preferred Supplier', 180, $scope.Suppliers);

            var commands = "";
            if ($rootScope.UserInfo.p_BRANDMASTER.modify)
                commands += "<input type='button' class='btn btn-primary btn-xs' value='Edit'/> &nbsp;"
            if ($rootScope.UserInfo.p_BRANDMASTER.delete)
                commands += "<input type='button' class='btn btn-danger btn-xs' value='Delete'/>"

            if ($rootScope.UserInfo.p_BRANDMASTER.modify || $rootScope.UserInfo.p_BRANDMASTER.delete) {
                igrid.AddButtonColumn('action', 'ACTION', 100, commands, function (e, args) {
                    $scope.$apply(function () {
                        if ($(e.target).val() == 'Edit')
                            $scope.ManageSearchItem('Edit', args.item);
                        else if ($(e.target).val() == 'Delete')
                            $scope.ManageSearchItem('Delete', args.item);
                    });
                });
            }
            igrid.options.autoEdit = true;
            igrid.onSelectedRangesChanged = function (range, rowmodel) {
                $timeout(function () {
                    $scope.selectedcount = range.length;
                }, 10);
            };
            igrid.Initialize();
            $scope.igrid = igrid;
            $scope.igrid.UpdateData($scope.SearchResult);

        }
        $timeout(function () {
            $scope.setFocusfirst();
            if (typeof ($location.search().id) != 'undefined' &&
              $location.search().id.length > 0) {
                request = {
                    "Item": { 'id': $location.search().id },
                    "SecurityToken": "String content"
                }

                var srequest = JSON.stringify(request);

                $http({
                    method: 'POST',
                    url: "BrandService/Search",
                    data: srequest
                }).success(function (result) {
                    $scope.ShowSearch = true;
                    if (angular.isArray(result)) {
                        $scope.SearchResult = result;
                    }

                }).error(function (result) {
                });
            }
        }, 100);
    }]);



