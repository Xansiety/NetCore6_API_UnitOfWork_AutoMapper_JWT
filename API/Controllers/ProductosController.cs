﻿using API.DTOs;
using AutoMapper;
using Core.Entities;
using Core.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers
{
    [Route("api/productos")]
    [ApiController]
    public class ProductosController : BaseApiController
    {

        private readonly IUnityOfWork _unityOfWork;
        private readonly IMapper _mapper;

        public ProductosController(IUnityOfWork unityOfWork, IMapper mapper)
        {
            _unityOfWork = unityOfWork;
            _mapper = mapper;
        }


        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<IEnumerable<ProductoListDTO>>> Get()
        {
            var productos = await _unityOfWork.Productos.GetAllAsync();
            return Ok(_mapper.Map<List<ProductoListDTO>>(productos));
        }

        [HttpGet("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<ProductoDTO>> Get(int id)
        {
            var producto = await _unityOfWork.Productos.GetByIdAsync(id); 
            if (producto is null) return NotFound();  
            return Ok(_mapper.Map<ProductoDTO>(producto));
        }


        [HttpPost]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<Producto>> Post(Producto producto)
        {
            _unityOfWork.Productos.Add(producto);
            await _unityOfWork.SaveAsync();
            if (producto is null) return BadRequest();

            return CreatedAtAction(nameof(Post), new { id = producto.Id }, producto);
        }


        //PUT: api/productos/28
        [HttpPut("{id}")]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<Producto>> Put(int id, [FromBody] Producto producto)
        {
            if (producto is null) return NotFound();

            _unityOfWork.Productos.Update(producto);
            await _unityOfWork.SaveAsync();
            if (producto is null) return BadRequest();

            return producto;
        }

        [HttpDelete("{id}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult> Delete(int id)
        {
            var producto = await _unityOfWork.Productos.GetByIdAsync(id);
            if (producto is null) return NotFound();

            _unityOfWork.Productos.Remove(producto);
            await _unityOfWork.SaveAsync();

            return NoContent();

        }
    }
}
