﻿using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using System.Web.Http.Description;
using ListIt.Api.Infrastructure;
using ListIt.Api.Models;

namespace ListIt.Api.Controllers
{
    public class ProductsController : ApiController
    {
        private ListItDataContext db = new ListItDataContext();

        // GET: api/Products
        [Authorize]
        public IHttpActionResult GetProducts()
        {
            var resultSet = db.Products.Select(p => new
                {
                    p.ProductId,
                    p.Name,
                    p.Description,
                    p.Posted,
                    p.Sold,
                    p.Active,
                    p.UserId,
                    p.Amount,
                    p.Condition,
                    Category = new
                    {
                        p.Category.Name,
                        p.Category.CategoryId
                    },
                    Photos = p.ProductPhotos.Select(pp => new
                    {
                        pp.Name,
                        pp.Url,
                        pp.Active
                    }),
                    ProductTag = p.ProductTags.Select(pt => new
                    {
                        pt.Tag.Name
                    })
                });

            return Ok(resultSet);
        }

        // GET: api/Products
        [Authorize, Route("api/products/search")]
        public IHttpActionResult GetSearchResults(string term)
        {
            var resultSet =
                db.Products.Where(p => p.Name.Contains(term) ||
                                       p.Category.Name.Contains(term) ||
                                       p.Description.Contains(term))
                           .Select(p => new
                        {
                            p.ProductId,
                            p.Name,
                            p.Description,
                            p.Posted,
                            p.Sold,
                            p.Active,
                            p.UserId,
                            p.Amount,
                            p.Condition,
                            Category = new
                            {
                                p.Category.Name,
                                p.Category.CategoryId
                            },
                            Photos = p.ProductPhotos.Select(pp => new
                            {
                                pp.Name,
                                pp.Url,
                                pp.Active
                            }),
                            ProductTag = p.ProductTags.Select(pt => new
                            {
                                pt.Tag.Name
                            })
                        });

            return Ok(resultSet);
        }

        // GET: api/Products/5
        [ResponseType(typeof(Product))]
        public IHttpActionResult GetProduct(int id)
        {
            Product product = db.Products.Find(id);
            if (product == null)
            {
                return NotFound();
            }

            return Ok(new
            {
                product.ProductId,
                product.Name,
                product.Amount,
                product.Condition,
                product.Description,
                Seller = new
                {
                    product.User.UserName,
                    product.User.Id,
                    product.User.ZipCode,
                    product.User.PhoneNumber
                },
                Category = new
                {
                    product.Category.Name,
                    product.Category.CategoryId
                },
                Photos = product.ProductPhotos.Select(pp => new
                    {
                        pp.Name,
                        pp.Url,
                        pp.ProductPhotoId,
                        pp.Active
                    })
                
             });
        }

        // PUT: api/Products/5
        [ResponseType(typeof(void))]
        public IHttpActionResult PutProduct(int id, Product product)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            if (id != product.ProductId)
            {
                return BadRequest();
            }

            // This is how we update stuff
            var dbProduct = db.Products.Find(id);

            dbProduct.Name = product.Name;
            dbProduct.Description = product.Description;
            dbProduct.Amount = product.Amount;
            dbProduct.Condition = product.Condition;
            dbProduct.CategoryId = product.Category.CategoryId;
            dbProduct.ProductPhotos = product.ProductPhotos;

            db.Entry(dbProduct).State = EntityState.Modified;

            try
            {
                db.SaveChanges();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!ProductExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return StatusCode(HttpStatusCode.NoContent);
        }

        // POST: api/Products
        [Authorize]
        [ResponseType(typeof(Product))]
        public IHttpActionResult PostProduct(Product product)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            string usernameFromToken = User.Identity.Name;

            var userFromDb = db.Users.First(u => u.UserName == User.Identity.Name);

            product.Posted = DateTime.Now;
            product.UserId = userFromDb.Id;

            db.Products.Add(product);
            db.SaveChanges();

            return CreatedAtRoute("DefaultApi", new { id = product.ProductId }, product);
        }

        // DELETE: api/Products/5
        [ResponseType(typeof(Product))]
        public IHttpActionResult DeleteProduct(int id)
        {
            Product product = db.Products.Find(id);
            if (product == null)
            {
                return NotFound();
            }

            db.Products.Remove(product);
            db.SaveChanges();

            return Ok(product);
        }

        // POST: api/Products/5/photo
        [HttpPost, Route("api/products/{id}/photo")]
        public IHttpActionResult PostProductPhoto(int id, ProductPhoto photo)
        {
            photo.ProductId = id;

            db.ProductPhotos.Add(photo);

            db.SaveChanges();

            return Ok(photo);
        }

        // DELETE: api/Products/5/photo
        [HttpDelete, Route("api/products/{id}/photo/{photoId}")]
        public IHttpActionResult RemoveProductPhoto(int id, int photoId)
        {
            ProductPhoto photo = db.ProductPhotos.Find(photoId);
            if (photo == null)
            {
                return NotFound();
            }

            db.ProductPhotos.Remove(photo);
            db.SaveChanges();

            return Ok();
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }

        private bool ProductExists(int id)
        {
            return db.Products.Count(e => e.ProductId == id) > 0;
        }
    }
}