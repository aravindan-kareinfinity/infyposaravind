
var app = angular.module('InfinityLab');

app.controller('SystemMapperController', ['UserDetails', '$scope', '$http', '$location', '$rootScope', '$window', 'restService', 'Excel', '$timeout', '$filter',
    function (UserDetails, $scope, $http, $location, $rootScope, $window, restService, Excel, $timeout, $filter) {
        $rootScope.AccordionName = $location.path();
        $scope.SearchRef = restService.SearchReference;

        $scope.Employee = restService.Get('ReferencelistService/EMPLOY', true).then(function (results) {
            $scope.Employee = results;
        });
        $scope.Sections = restService.Get('ReferencelistService/DIVISION', true).then(function (results) {
            $scope.SupplierGroups = results;
        });
        $scope.Products = restService.Get('ReferencelistService/PRODUCTS', true).then(function (results) {
            $scope.Products = results;
        });
        $scope.Suppliers = restService.Get('ReferencelistService/SUPPLIER', true).then(function (results) {
            $scope.Suppliers = results;
        });

        $scope.Company = restService.Get('ReferencelistService/BUYINGCOMPANY', true).then(function (results) {
            $scope.Company = results;
        });
        $scope.Locations = restService.Get('ReferencelistService/LOCATION', true).then(function (results) {
            $scope.Locations = results;
        });

        $scope.CurrentSource = [];
        $scope.SelectedEntity = { 'Mode': 0, 'ParentID': 0, 'CanSave': false, 'CanCancel': false, 'CanSearch': true };

        $scope.Modes = [{ 'id': 1, 'name': 'Employee -> Product' },
            { 'id': 2, 'name': 'Sections -> Supplier' }];



        $scope.Entity = {
            "type": 0,
            "mapid": 0,
            "parentid": 0,
            "isactive": true,
            "createdby": 0,
            "version": 1,
            "createdon": "0001-01-01T00:00:00",
            "modifiedon": "0001-01-01T00:00:00",
            "childids": { "items": [] },

        };

        $scope.SystemMapper = (JSON.parse(JSON.stringify($scope.Entity)));

        $scope.ErrorStyle = "success";
        $scope.ShowSearch = false;
        $scope.allattribute = false;
        $scope.Source = [];
        $scope.Selected = [];

        $scope.Move = function (Action, item, from, to) {
            var index = from.indexOf(item);

            to.push(item);
            from.splice(from.indexOf(item), 1);

            $scope.SortProducts($scope.Source);
            $scope.SortProducts($scope.Selected);
            $scope.igrid.UpdateData($scope.Source);
            $scope.sgrid.UpdateData($scope.Selected);
        }

        $scope.MoveAll = function (grid, from, to) {
            $.each(grid.GetSelectedRows(), function (k, v) {
                to.push(v);
                from.splice(from.indexOf(v), 1);
            })
            $scope.SortProducts($scope.Source);
            $scope.SortProducts($scope.Selected);
            $scope.igrid.UpdateData($scope.Source);
            $scope.sgrid.UpdateData($scope.Selected);
        }

        $scope.OnMappingModeChange = function (type) {
            $scope.Selected = [];
            $scope.Source = [];

            if (type == '1') {
                $scope.CurrentSource = $scope.Employee;
                $.each($scope.Products, function (k, v) {
                    $scope.Source.push({
                        'childid': v.id, 'code': v.code, 'name': v.name,
                        'parentname': '', 'parentid': 0, 'orderid': k
                    });
                });
            }
            if (type == '2') {
                $scope.CurrentSource = $scope.SupplierGroups;
                $.each($scope.Suppliers, function (k, v) {
                    $scope.Source.push({
                        'childid': v.id, 'code': v.code, 'name': v.name, 'parentname': '',
                        'parentid': 0, 'orderid': k
                    });
                });
            }
            $scope.SearchChildrens();
        }

        $scope.ManageProductList = function () {
            if ($scope.igrid != null)
                $scope.igrid.destroy();

            if ($scope.sgrid != null)
                $scope.sgrid.destroy();

            var igrid = new InfyGrid('gridContainer', '450px', '100%', true, true, true, 50);
            igrid.options.showHeaderRow = true;

            igrid.AddSimpleColumn('code', 'Code', 75);
            igrid.AddSimpleColumn('name', 'Name', 200);
            igrid.AddSimpleColumn('parentname', 'Link', 200);

            igrid.AddButtonColumn('action', ' ', 40, '->', function (e, args) {
                $scope.$apply(function () {
                    if ($scope.SelectedEntity.CanSave) {
                        $scope.Move('From', args.item, $scope.Source, $scope.Selected);
                    }
                    else {
                        $rootScope.Toaster.Info("Please search before move!");
                    }
                });
            });
            igrid.Initialize();
            $scope.igrid = igrid;

            var sgrid = new InfyGrid('gridContainerselected', '450px', '100%', true, true, true, 50);
            sgrid.options.showHeaderRow = true;

            sgrid.AddSimpleColumn('code', 'Code', 150);
            sgrid.AddSimpleColumn('name', 'Name', 300);


            sgrid.AddButtonColumn('action', ' ', 40, '&lt;-', function (e, args) {
                $scope.$apply(function () {
                    if ($scope.SelectedEntity.CanSave) {
                        $scope.Move('To', args.item, $scope.Selected, $scope.Source);
                    }
                    else {
                        $rootScope.Toaster.Info("Please search before move!");
                    }
                });
            });
            sgrid.Initialize();
            $scope.sgrid = sgrid;
        }

        $scope.ManageProductList();

        $scope.Cancel = function () {
            $scope.SelectedEntity.CanCancel = false;
            $scope.SelectedEntity.CanSave = false;
        }


        $scope.SortProducts = function (array) {
            array.sort(function (a, b) {
                if (a.orderid < b.orderid) { return -1; }
                if (a.orderid > b.orderid) { return 1; }
                return 0;
            });
        }

        $scope.LoadChildrens = function () {
            $.each($scope.Selected, function (k, v) {
                $scope.Source.push(v);
            });

            $scope.Selected.splice(0, $scope.Selected.length);

            if ($scope.SystemMapper.parentid && $scope.SystemMapper.parentid != null) {
                $scope.SelectedEntity.CanCancel = true;
                $scope.SelectedEntity.CanSave = true;

                $.each($scope.Source, function (k, v) {
                    if (v.parentid == $scope.SystemMapper.parentid) {
                        $scope.Selected.push(v);
                    }
                });

                $.each($scope.Selected, function (k, v) {
                    $scope.Source.splice($scope.Source.indexOf(v), 1);
                });
            }
            $scope.SortProducts($scope.Source);
            $scope.SortProducts($scope.Selected);
            $scope.igrid.UpdateData($scope.Source);
            $scope.sgrid.UpdateData($scope.Selected);
        }

        $scope.SearchChildrens = function () {

            $scope.Selected = [];
            request = {
                "Item": {
                    'type': $scope.SystemMapper.type
                },
                "securitytoken": "String content"
            }

            var srequest = JSON.stringify(request);

            $http({
                method: 'POST',
                url: "SystementitymapperService/Search",
                data: request
            }).success(function (result) {
                if (result != null && result.length > 0) {
                    $.each($scope.Selected, function (k, v) {
                        $scope.Source.push(v);
                    });

                    $scope.Selected.splice(0, $scope.Selected.length);

                    $.each(result, function (k, v) {
                        var exist = $scope.Source.find(function (item) {
                            return item.childid == v.childid;
                        });
                        if (typeof (exist) != 'undefined' && exist != null) {
                            var parent = $scope.CurrentSource.find(function (item) {
                                return item.id == v.parentid;
                            });
                            if (parent && parent != null) {
                                exist.parentname = parent.name;
                                exist.parentid = parent.id;
                            }
                        }
                    });
                }
                $scope.SortProducts($scope.Source);
                $scope.SortProducts($scope.Selected);
                $scope.igrid.UpdateData($scope.Source);
                $scope.sgrid.UpdateData($scope.Selected);
                }).error(function (result) {
            });
        }

        $scope.Save = function () {

            $scope.SystemMapper.parentname = $scope.SearchRef($scope.SystemMapper.parentid, $scope.CurrentSource, '');
            $scope.SystemMapper.childids = [];
            $.each($scope.Selected, function (k, v) {
                $scope.SystemMapper.childids.push({ 'id': v.childid, 'code': v.code, 'name': v.name });
            });
            request = {
                "Item": $scope.SystemMapper,
                "securitytoken": "String content"
            }

            var srequest = JSON.stringify(request);

            $http({
                method: 'POST',
                url: "SystementitymapperService/UpdateMapping",
                data: request
            }).success(function (result) {

                if (result != null && result) {
                    $.each($scope.Source, function (k, v) {
                        if (v.parentid == $scope.SystemMapper.parentid) {
                            v.parentid = 0;
                            v.parentname = '';
                        }
                    });
                    $.each($scope.Selected, function (k, v) {
                        v.parentid = $scope.SystemMapper.parentid;
                        v.parentname = $scope.SystemMapper.parentname;
                    });
                    $scope.SortProducts($scope.Source);
                    $scope.SortProducts($scope.Selected);
                    $scope.igrid.UpdateData($scope.Source);
                    $scope.sgrid.UpdateData($scope.Selected);
                    $rootScope.Toaster.Info("System Mapper Saved Successfully.!");
                }
                else {
                    $rootScope.Toaster.Error("Can't save the system mappings");
                }

            }).error(function (result) {
            });

        }


    }]);