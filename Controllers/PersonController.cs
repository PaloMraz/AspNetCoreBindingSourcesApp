using AspNetCoreBindingSourcesApp.Models;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AspNetCoreBindingSourcesApp.Controllers
{
    public class PersonController : Controller
    {
        public IActionResult Index()
        {
            return this.View();
        }


        [HttpPost]
        public IActionResult PostAsJson([FromBody] PersonViewModel model)
        {            
            return this.Ok($"{model} (posted to {nameof(PostAsJson)})");
        }


        [HttpPost]
        public IActionResult PostAsForm(PersonViewModel model)
        {
           return this.Ok($"{model} (posted to {nameof(PostAsForm)})");
        }
    }
}
