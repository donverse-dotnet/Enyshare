import React from "react";
import { Outlet, Link } from "react-router-dom";

export default function DocumentLayout() {
    return (
        <>
            <header style={{ padding: 10, borderBottom: "1px solid #ddd", height: "30px" }}>
                <Link to="/">Home</Link> | <Link to="/about">About</Link> | <Link to="/contact">Contact</Link>
            </header>
            <main style={{ display: "flex", height: "100%" }}>
                <aside style={{
                    width: "200px",
                    height: "100%",
                    float: "left",
                    borderRight: "1px solid #ddd",
                    overflowY: "auto",
                    padding: "10px"
                }}>
                    <nav>
                        <ul style={{ listStyle: "none", padding: 0 }}>
                            <li><Link to="/docs">Documentation</Link></li>
                            <li><Link to="/guides">Guides</Link></li>
                            <li><Link to="/api">API Reference</Link></li>
                        </ul>
                    </nav>
                </aside>
                <div style={{ marginLeft: "220px" }}>
                    <Outlet />
                </div>
            </main>
        </>
    );
}
