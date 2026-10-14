
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;
using InfyPOS.Models;
using InfyPOS.Common;
using InfyPOS.Data;
using Microsoft.Owin.Security;
using System.Security.Claims;

namespace InfyPOS.Controllers
{
    //[Authorize]
    [RoutePrefix("ReferencelistService")]
    public class ReferencelistServiceAPIController : ApiController
    {
        private IAuthenticationManager Authentication
        {
            get {
                ClaimsPrincipal principal = Request.GetRequestContext().Principal as ClaimsPrincipal;
                return Request.GetOwinContext().Authentication;
                
            }
        }

        [HttpGet]
        public async Task<IHttpActionResult> Get(string id)
        {
            
            if (id == "SUPPLIER")
            {
                SupplierDataService sDataService = new SupplierDataService();
                List<Supplier> suppliers = await Task<List<Supplier>>.Run(() => sDataService.Search(new SupplierCreteria()));
                var list = suppliers.ConvertAll<ReferenceItem>(e => new ReferenceItem
                {
                    id = e.id,
                    name = e.name
                });
                return this.Ok(list);
            }
            if (id == "TRANSPORT")
            {
                TransportDataService sDataService = new TransportDataService();
                List<Transport> transports = await Task<List<Transport>>.Run(() => sDataService.Search(new TransportCreteria()));
                var list = transports.ConvertAll<ReferenceItem>(e => new ReferenceItem
                {
                    id = e.id,
                    name = e.name
                });
                return this.Ok(list);
            }
            if (id == "SUPPLIERCOMPANY")
            {
                SuppliercompanyDataService sDataService = new SuppliercompanyDataService();
                List<Suppliercompany> suppliercompany = await Task<List<Suppliercompany>>.Run(() => sDataService.Search(new SuppliercompanyCreteria()));
                var list = suppliercompany.ConvertAll<ReferenceItem>(e => new ReferenceItem
                {
                    id = e.id,
                    name = e.name
                });
                return this.Ok(list);
            }
            if (id == "BUYINGCOMPANY")
            {
                CompanyDataService sDataService = new CompanyDataService();
                List<Company> buyingcompany = await Task<List<Company>>.Run(() => sDataService.Search(new CompanyCreteria()));
                var list = buyingcompany.ConvertAll<ReferenceItem>(e => new ReferenceItem
                {
                    id = e.id,
                    name = e.name
                });
                return this.Ok(list);
            }
            if (id == "PRODUCTS")
            {
                ProductsDataService sDataService = new ProductsDataService();
                List<Products> buyingcompany = await Task<List<Products>>.Run(() => sDataService.Search(new ProductsCreteria()));
                var list = buyingcompany.ConvertAll<ReferenceItem>(e => new ReferenceItem
                {
                    id = e.id,
                    name = e.name
                });
                return this.Ok(list);
            }
            if (id == "AGENT")
            {
                AgentDataService sDataService = new AgentDataService();
                List<Agent> agents = await Task<List<Agent>>.Run(() => sDataService.Search(new AgentCreteria()));
                var list = agents.ConvertAll<ReferenceItem>(e => new ReferenceItem
                {
                    id = e.id,
                    name = e.name
                });
                return this.Ok(list);
            }
            if (id == "EMPLOY")
            {
                EmployeeDataService sDataService = new EmployeeDataService();
                List<Employee> buyingcompany = await Task<List<Employee>>.Run(() => sDataService.Search(new EmployeeCreteria()));
                var list = buyingcompany.ConvertAll<ReferenceItem>(e => new ReferenceItem
                {
                    id = e.id,
                    name = e.name
                });
                return this.Ok(list);
            }

            ReferencelistDataService ReferencelistData = new ReferencelistDataService();
            ReferencelistCreteria creteria = new ReferencelistCreteria();
            creteria.type = id;
            List<ReferenceItem> result = await Task<List<ReferenceItem>>.Run(() => ReferencelistData.SearchList(creteria));
            return this.Ok(result);
        }

        [HttpGet]
        [Route("Entity")]
        public async Task<IHttpActionResult> Entity()
        {
            var result = new ActionRequest<Referencelist>();
            result.Item = new Referencelist();
            return this.Ok(result);
        }

        [HttpPost]
        [Route("Search")]
        public async Task<IHttpActionResult> Search(ActionRequest<ReferencelistCreteria> request)
        {
            ReferencelistDataService ReferencelistData = new ReferencelistDataService();
            List<Referencelist> result = await Task<List<Referencelist>>.Run(() => ReferencelistData.Search(request.Item));
            return this.Ok(result);
        }

        [HttpPost]
        [Route("Save")]
        public async Task<IHttpActionResult> Save(ActionRequest<Referencelist> request)
        {
            ReferencelistDataService ReferencelistData = new ReferencelistDataService();
            if (request.Item.id <= 0)
            {
                Referencelist result = await Task<Referencelist>.Run(() => ReferencelistData.Insert(request.Item));
                return this.Ok(result);
            }
            else
            {
                Referencelist result = await Task<Referencelist>.Run(() => ReferencelistData.Update(request.Item));
                return this.Ok(result);
            }
        }

        [HttpPost]
        [Route("Insert")]
        public async Task<IHttpActionResult> Insert(ActionRequest<Referencelist> request)
        {
            ReferencelistDataService ReferencelistData = new ReferencelistDataService();
            Referencelist result = await Task<Referencelist>.Run(() => ReferencelistData.Insert(request.Item));
            return this.Ok(result);
        }

        [HttpPost]
        [Route("Update")]
        public async Task<IHttpActionResult> Update(ActionRequest<Referencelist> request)
        {
            ReferencelistDataService ReferencelistData = new ReferencelistDataService();
            Referencelist result = await Task<Referencelist>.Run(() => ReferencelistData.Update(request.Item));
            return this.Ok(result);
        }

        [HttpPost]
        [Route("Delete")]
        public async Task<IHttpActionResult> Delete(ActionRequest<Referencelist> request)
        {
            ReferencelistDataService ReferencelistData = new ReferencelistDataService();
            bool result = await Task<bool>.Run(() => ReferencelistData.Delete(request.Item));
            return this.Ok(result);
        }
    }
}